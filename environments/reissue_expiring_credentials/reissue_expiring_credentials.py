################################################################################
# Copyright (c) 2025 Cofinity-X GmbH
# Copyright (c) 2025 Contributors to the Eclipse Foundation
#
# See the NOTICE file(s) distributed with this work for additional
# information regarding copyright ownership.
#
# This program and the accompanying materials are made available under the
# terms of the Apache License, Version 2.0 which is available at
# https://www.apache.org/licenses/LICENSE-2.0.
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
# WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
# License for the specific language governing permissions and limitations
# under the License.
#
# SPDX-License-Identifier: Apache-2.0
################################################################################

import argparse
import logging
from argparse import Namespace
from datetime import date
from enum import Enum
from time import time
from typing import Dict, List
from urllib.parse import urljoin

import requests
from requests import JSONDecodeError, Response

### CONSTANTS
# URLs and paths
DID_DOCUMENT_BASE_URL = "https://portal-backend.{}.catena-x.net/api/administration/staticdata/did/{}/did.json"
ISSUER_SERVICE_BASE_URL = "https://ssi-credential-issuer.{}.catena-x.net"
KEYCLOAK_BASE_URL = "https://centralidp.{}.catena-x.net"
AUTH_TOKEN_PATH_KEYCLOAK = "/auth/realms/CX-Central/protocol/openid-connect/token"
AUTH_TOKEN_PATH_SAP = "/oauth/token"

# JSON fields
APPLICATION_JSON = "application/json"
APPLICATION_X_WWW_FORM_URLENCODED = "application/x-www-form-urlencoded"
ACCEPT = "accept"
CONTENT_TYPE = "Content-Type"
AUTHORIZATION = "Authorization"
BEARER_TOKEN = "Bearer {}"
URL = "url"
CLIENT_ID = "client_id"
CLIENT_SECRET = "client_secret"

# Dict keys
KEY_CREDENTIAL_ID = "credential_id"
KEY_TYPE = "type"
KEY_OPERATION_ID = "operation_id"
KEY_BPN = "bpn"
KEY_HOLDER_DID = "holder_did"
KEY_CUSTOMER_NAME = "customer_name"


class CredentialType(Enum):
    BUSINESS_PARTNER_NUMBER = "BUSINESS_PARTNER_NUMBER"
    MEMBERSHIP_CREDENTIAL = "MEMBERSHIP"
    DATA_EXCHANGE_GOVERNANCE_CREDENTIAL = "DATA_EXCHANGE_GOVERNANCE_CREDENTIAL"


CLIENT_INFO_CACHE = {}
TOKEN_CACHE = {}


def setup_logger(log_level: int | str) -> logging.Logger:
    """
    Setup logger with specified level.
    If `log_level` is an int, it must be one of 10, 20, 30, 40, 50.
    If `log_level` is a str, it must be one of "DEBUG", "INFO", "WARN", "WARNING", "ERROR", or "CRITICAL", "FATAL".
    """

    if isinstance(log_level, str):
        numeric_log_level = getattr(logging, log_level.upper(), None)
        if not isinstance(numeric_log_level, int):
            raise ValueError(f"Invalid log level: {log_level}")
        level = numeric_log_level
    elif isinstance(log_level, int):
        level = log_level
    else:
        raise TypeError(f"`log_level` must be of type int | str, but was: {type(log_level)}")

    logging.basicConfig(
        level=level,
        format="%(asctime)s - %(name)s - %(levelname)s - %(message)s"
    )
    return logging.getLogger(__name__)


def fetch_active_credentials(auth_base_url: str, client_id: str, client_secret: str, issuer_url: str) -> List[Dict]:
    logging.debug("Fetching expiring credentials")
    issuer_service_auth_token = get_auth_token(
        auth_base_url,
        AUTH_TOKEN_PATH_KEYCLOAK,
        client_id,
        client_secret,
        True,
    )
    headers = {
        ACCEPT: APPLICATION_JSON,
        AUTHORIZATION: BEARER_TOKEN.format(issuer_service_auth_token),
    }

    current_page = 1
    total_num_pages = 1
    credential_list_acc: list = []

    while current_page <= total_num_pages:
        # Execute request and ensure status 200
        response: Response = requests.get(
            url=f"{issuer_url}/api/issuer?page={current_page}&size=15&companySsiDetailStatusId=ACTIVE",
            headers=headers
        )
        if response.status_code != 200:
            raise requests.HTTPError(
                f"Failed to fetch credentials on page {current_page}: {response.status_code} {response.text}")

        # Extract response body
        try:
            response_json = response.json()
        except JSONDecodeError as jsonDecodeError:
            logging.error(
                f"Issuer service returned 200, but the response contained invalid JSON for page={current_page}: {response.text}")
            raise jsonDecodeError
        # Determine total number of pages
        if current_page == 1:
            try:
                total_num_pages = response_json["meta"]["totalPages"]
            except JSONDecodeError as jsonDecodeError:
                logging.error(f"Failed to parse [meta][totalPages] from response for page=1: {response.text}")
                raise jsonDecodeError

        # Extract credentials from current page
        try:
            credentials_current_page = response_json["content"]
        except JSONDecodeError as jsonDecodeError:
            logging.error(f"Failed to extract credentials from response for page={current_page}: {response.text}")
            raise jsonDecodeError

        # Append credentials to accumulator list
        credential_list_acc.extend(credentials_current_page)

        current_page += 1

    return credential_list_acc


def expires_in_range(credential: dict, expiry_on_or_after: date, expiry_on_or_before: date) -> bool:
    """
    Returns `true` if the credential expiry date lies between `expiry_on_or_after` (inclusive) and `expiry_on_or_before` (inclusive).
    False otherwise.
    Also returns false if credential has no expiry date.
    """
    expiry_date_str = credential.get("expiryDate")
    if expiry_date_str is None:
        logging.warning(
            f"No expiry date found for credential ID: {credential.get("credentialDetailId")}. Treating it as expiring within given range.")
        return False

    try:
        expiry_date = date.fromisoformat(expiry_date_str.split("T")[0])
    except ValueError as valueError:
        raise ValueError(f"Invalid 'expiryDate' value in credential:\n{credential}\n{valueError}")

    return expiry_on_or_after <= expiry_date <= expiry_on_or_before


def verify_create_signed_credential_step_done(credential: dict) -> bool:
    process_steps = credential["processSteps"]
    for step in process_steps:
        process_step_type_id = step["processStepTypeId"]
        process_step_status_id = step["processStepStatusId"]
        if process_step_type_id == "CREATE_SIGNED_CREDENTIAL" and process_step_status_id == "DONE":
            return True
    return False


def filter_credentials(credentials: list, start: date, end: date) -> List[Dict]:
    filtered_credentials = []
    for cred in credentials:
        if expires_in_range(cred, start, end) and verify_create_signed_credential_step_done(cred):
            filtered_credentials.append(cred)
    return filtered_credentials


def transform_credential_data(cred: dict, stage: str) -> dict:
    try:
        bpn = cred["bpnl"]
        credential_type = cred["credentialType"]
        credential_detail_id = cred["credentialDetailId"]
    except KeyError as keyError:
        raise KeyError(f"Credential does not have required fields: {cred}\n{keyError}")
    return {
        KEY_BPN: bpn,
        KEY_TYPE: credential_type,
        KEY_HOLDER_DID: DID_DOCUMENT_BASE_URL.format(stage, bpn),
        KEY_CREDENTIAL_ID: credential_detail_id,
    }


def get_operation_ids(sap_auth_url: str, sap_client_id: str, sap_client_secret: str, sap_url: str) -> List[Dict]:
    """Fetch operation IDs"""
    sap_token = get_auth_token(
        sap_auth_url,
        AUTH_TOKEN_PATH_SAP,
        sap_client_id,
        sap_client_secret
    )

    headers = {AUTHORIZATION: BEARER_TOKEN.format(sap_token)}

    response = requests.get(f"{sap_url}/api/v1.0.0/customerWallets", headers=headers)
    if response.status_code != 200:
        raise requests.HTTPError(f"Failed to get customer wallets: {response.status_code} {response.text}")

    response_json = response.json()
    try:
        response_data = response_json["data"]
    except KeyError as key_error:
        logging.error(f"Could not access `data` field in response: {response_json}")
        raise key_error

    operation_ids = []
    for customer in response_data:
        try:
            customer_name = customer["customerName"]
            operation_id = customer["lastOperationId"]
        except KeyError as key_error:
            logging.error(f"Could not access `customerName` or `lastOperationId` fields in response: {customer}.\n"
                          f"Full response: {response_json}")
            raise key_error
        operation_ids.append({
            KEY_CUSTOMER_NAME: customer_name,
            KEY_OPERATION_ID: operation_id,
        })

    return operation_ids


def add_operation_id_to_credential_data(
        stage: str,
        sap_auth_url: str,
        sap_client_id: str,
        sap_client_secret: str,
        sap_url: str,
        credentials: List[Dict],
        operation_data: List[Dict],
) -> List[Dict]:
    logging.debug("Merging credential data with operation IDs")
    merged_data = []
    num_removed_credential = 0
    for cred in credentials:
        bpn = cred[KEY_BPN]
        op_ids = []
        for operation in operation_data:
            if bpn in operation[KEY_CUSTOMER_NAME]:
                op_ids.append(operation[KEY_OPERATION_ID])

        if len(op_ids) == 1:
            op_id = op_ids[0]
        elif len(op_ids) > 1:
            op_id = get_first_op_id_by_stage(stage, sap_auth_url, sap_client_id, sap_client_secret, sap_url, op_ids)
        else:
            op_id = ""

        if op_id:
            cred.update({KEY_OPERATION_ID: op_id})
            merged_data.append(cred)
        else:
            num_removed_credential += 1
            logging.warning(
                f"Removed credential - No operation ID found for BPN: {bpn},"
                f" Type: {cred["type"]}, Credential ID: {cred[KEY_CREDENTIAL_ID]}",
            )

    if num_removed_credential > 0:
        logging.warning(f"Total {num_removed_credential} credentials removed due to missing operation IDs:")
    return merged_data


def get_first_op_id_by_stage(
        stage: str,
        sap_auth_url: str,
        sap_client_id: str,
        sap_client_secret: str,
        sap_url: str,
        matching_op_ids: List[str],
) -> str:
    """
    Returns the first operation ID valid for the given stage.
    If no valid operation ID is found, returns an empty string.
    """
    for operation_id in matching_op_ids:
        company_did: str = get_company_did(
            sap_auth_url,
            sap_client_id,
            sap_client_secret,
            sap_url,
            operation_id
        )
        if stage in company_did:
            return operation_id
    return ""


def get_auth_token(auth_base_url: str, auth_path: str, client_id: str, client_secret: str,
                   is_user: bool = False) -> str:
    """Get authentication token from Keycloak with caching"""
    cache_key = f"{auth_base_url}:{client_id}"

    # Check cache and token expiry
    if cache_key in TOKEN_CACHE:
        token_data = TOKEN_CACHE[cache_key]
        # Check if token is still valid (with 15s buffer)
        if token_data["expiry"] > time() + 15:
            logging.debug(f"Using cached token for {cache_key}")
            return token_data["token"]
        else:
            logging.debug(f"Cached token expired for {cache_key}")
            TOKEN_CACHE.pop(cache_key)

    headers = {CONTENT_TYPE: APPLICATION_X_WWW_FORM_URLENCODED}

    if is_user:
        payload = f"username={client_id}&grant_type=password&client_id=Cl2-CX-Portal&password={client_secret}"
    else:
        payload = f"client_id={client_id}&grant_type=client_credentials&client_secret={client_secret}"

    logging.debug(f"Getting token from {urljoin(auth_base_url, auth_path)}")
    response: Response = requests.post(urljoin(auth_base_url, auth_path), headers=headers, data=payload)
    if response.status_code != 200:
        raise requests.HTTPError(f"Failed to get access token: {response.status_code} {response.text}")

    body = response.json()

    # Cache the token with expiry
    TOKEN_CACHE[cache_key] = {
        "token": body["access_token"],
        "expiry": time() + body.get("expires_in", 300)  # Default 5 min if expires_in not present
    }

    logging.debug(f"Cached new token for {cache_key}")
    return body["access_token"]


def get_customer_client_info(
        sap_auth_url: str,
        sap_client_id: str,
        sap_client_secret: str,
        sap_base_url: str,
        operation_id: str,
) -> Dict:
    """Get Customer client information for a specific operation with local caching"""
    if operation_id in CLIENT_INFO_CACHE:
        logging.debug(f"Using cached client info for operation ID: {operation_id}")
        return CLIENT_INFO_CACHE[operation_id]

    sap_auth_token = get_auth_token(
        sap_auth_url,
        AUTH_TOKEN_PATH_SAP,
        sap_client_id,
        sap_client_secret
    )
    response = requests.get(
        f"{sap_base_url}/api/v1.0.0/operations/{operation_id}",
        headers={AUTHORIZATION: BEARER_TOKEN.format(sap_auth_token)},
    )
    if response.status_code != 200:
        raise requests.HTTPError(
            f"Failed to get operation details for operation ID {operation_id}: {response.status_code} {response.text}")

    try:
        response_json = response.json()
    except JSONDecodeError as e:
        logging.error(f"Failed to parse JSON response. Full response text: {response.text}")
        raise e

    try:
        uaa = response_json["data"]["serviceKey"]["uaa"]
    except KeyError as e:
        logging.error(
            f"Failed to parse [data][serviceKey][uaa] field from response. Is the operation status completed? Full response text: {response.text}")
        raise e

    try:
        url = uaa[URL]
        client_id = uaa["clientid"]
        client_secret = uaa["clientsecret"]
    except KeyError as e:
        logging.error("Failed to extract client info from uaa.")
        raise e

    client_info = {
        URL: url,
        CLIENT_ID: client_id,
        CLIENT_SECRET: client_secret
    }
    CLIENT_INFO_CACHE[operation_id] = client_info
    logging.debug(f"Cached client info for operation ID: {operation_id}")

    return client_info


def get_company_did(
        sap_auth_url: str,
        sap_client_id: str,
        sap_client_secret: str,
        sap_url: str,
        operation_id: str
) -> str:
    """Get Company DID"""
    customer_client_info: dict = get_customer_client_info(
        sap_auth_url,
        sap_client_id,
        sap_client_secret,
        sap_url,
        operation_id
    )

    auth_token = get_auth_token(
        customer_client_info[URL],
        AUTH_TOKEN_PATH_SAP,
        customer_client_info[CLIENT_ID],
        customer_client_info[CLIENT_SECRET]
    )

    response = requests.get(
        "https://dis-integration-service-prod.eu10.div.cloud.sap/api/v2.0.0/companyIdentities",
        headers={AUTHORIZATION: BEARER_TOKEN.format(auth_token)},
    )

    if response.status_code != 200:
        raise requests.HTTPError(
            f"Failed to get company DID for operation ID {operation_id}: {response.status_code} {response.text}")

    try:
        response_json = response.json()
    except JSONDecodeError as e:
        logging.error(f"Failed to parse JSON response. Full response text: {response.text}")
        raise e

    try:
        if not response_json["data"]:
            logging.warning(f"No data found for the given BPN. Full response text: {response.text}")
            return None
        return response_json["data"][0]["issuerDID"]
    except KeyError as e:
        logging.error(
            f"Failed to parse [data][0][issuerDID] field from response. Full response text: {response.text}")
        raise e


def issue_credential(
        auth_url: str,
        issuer_service_client_id: str,
        issuer_service_client_secret: str,
        issuer_url: str,
        cred_type: str,
        holder_did: str,
        bpn: str,
        wallet_url: str,
        tech_user_client_id: str,
        tech_user_client_secret: str,
):
    # Create base payload for issuance request
    base_payload = {
        "holder": holder_did,
        "businessPartnerNumber": bpn,
        "technicalUserDetails": {
            "walletUrl": wallet_url,
            "clientId": tech_user_client_id,
            "clientSecret": tech_user_client_secret
        },
        "callbackUrl": None,
    }

    # Set URL and amend payload based on credential type
    if cred_type == CredentialType.BUSINESS_PARTNER_NUMBER.value:
        url = f"{issuer_url}/api/issuer/bpn"
    elif cred_type == CredentialType.MEMBERSHIP_CREDENTIAL.value:
        url = f"{issuer_url}/api/issuer/membership"
        base_payload["memberOf"] = "catena-x"
    elif cred_type == CredentialType.DATA_EXCHANGE_GOVERNANCE_CREDENTIAL.value:
        url = f"{issuer_url}/api/issuer/framework"
        base_payload.update({
            "useCaseFrameworkId": "DATA_EXCHANGE_GOVERNANCE_CREDENTIAL",
            "useCaseFrameworkVersionId": "090efafd-9667-404f-85cc-d7d072b5ad46"
        })
    else:
        raise ValueError(f"Unsupported credential type: {cred_type}")

    # Get auth token for issuer service
    issuer_auth_token = get_auth_token(
        auth_url,
        AUTH_TOKEN_PATH_KEYCLOAK,
        issuer_service_client_id,
        issuer_service_client_secret,
        True
    )

    headers = {
        ACCEPT: APPLICATION_JSON,
        CONTENT_TYPE: APPLICATION_JSON,
        AUTHORIZATION: BEARER_TOKEN.format(issuer_auth_token),
    }
    response = requests.post(url, headers=headers, json=base_payload)
    if response.status_code != 200:
        raise requests.HTTPError(f"Failed to issue credential: {response.status_code} {response.text}")
    logging.info(f"Issued new {cred_type} credential with ID {response.text} to {holder_did}.")


def revoke_credential(auth_url: str, client_id: str, client_secret: str, issuer_url: str, credential_id: str):
    """Revoke old credential"""
    issuer_token = get_auth_token(
        auth_url,
        AUTH_TOKEN_PATH_KEYCLOAK,
        client_id,
        client_secret,
        True
    )
    headers = {
        ACCEPT: APPLICATION_JSON,
        AUTHORIZATION: BEARER_TOKEN.format(issuer_token),
    }

    response = requests.post(
        f"{issuer_url}/api/revocation/issuer/credentials/{credential_id}",
        headers=headers
    )
    if response.status_code != 200:
        raise requests.HTTPError(f"Failed to revoke credential: {credential_id} {response.status_code} {response.text}")


def parse_cli_args() -> tuple[Namespace, list[str]]:
    parser = argparse.ArgumentParser(description="Reissue expiring credentials")
    parser.add_argument("--stage", required=True, choices=["int"], help="Environment stage")
    parser.add_argument("--start_date", type=date.fromisoformat, required=True,
                        help="yyyy-mm-dd - Credentials expiring on or after this date and on or before end_date will be reissued.")
    parser.add_argument("--end_date", type=date.fromisoformat, required=True,
                        help="yyyy-mm-dd - Credentials expiring on or before this date and on or after start_date will be reissued.")
    parser.add_argument("--issuer-service-client-id", required=True, help="SSI Issuer client ID")
    parser.add_argument("--issuer-service-client-secret", required=True, help="SSI Issuer client secret")
    parser.add_argument("--sap-url", required=True, help="SAP base URL")
    parser.add_argument("--sap-auth-url", required=True, help="SAP auth URL")
    parser.add_argument("--sap-client-id", required=True, help="SAP client ID")
    parser.add_argument("--sap-client-secret", required=True, help="SAP client secret")
    parser.add_argument("--log-level", default="DEBUG", choices=["DEBUG", "INFO", "WARNING", "ERROR"],
                        help="Logging level")
    parser.add_argument("--limit", type=int, help="Maximum number of credentials to reissue")
    return parser.parse_known_args()


def main():
    # Unpack args
    args, unknown_args = parse_cli_args()

    stage = args.stage
    start_date: date = args.start_date
    end_date: date = args.end_date
    issuer_service_client_id = args.issuer_service_client_id
    issuer_service_client_secret = args.issuer_service_client_secret
    sap_url = args.sap_url
    sap_auth_url = args.sap_auth_url
    sap_client_id = args.sap_client_id
    sap_client_secret = args.sap_client_secret
    log_level = args.log_level
    iter_limit = args.limit

    # Setup logging
    logger = setup_logger(log_level)
    logging.info(
        f"Running with the following arguments:\n"
        f"stage: {stage}\n"
        f"start_date: {start_date}\n"
        f"end_date: {end_date}\n"
        f"issuer_service_client_id: {issuer_service_client_id}\n"
        f"sap_url: {sap_url}\n"
        f"sap_auth_url: {sap_auth_url}\n"
        f"sap_client_id: {sap_client_id}\n"
        f"log_level: {log_level}\n"
        f"limit: {iter_limit}\n"
    )
    logging.info(
        f"Reissuing credentials that are active and expire between {start_date} and {end_date} (both inclusive).",
    )
    if len(unknown_args) > 0:
        logging.warning(f"Found {len(unknown_args)} unknown arguments, ignoring them")

    # Setup base URLs
    issuer_service_base_url = ISSUER_SERVICE_BASE_URL.format(stage)
    keycloak_base_url = KEYCLOAK_BASE_URL.format(stage)

    # Fetch all active credentials
    active_credentials = fetch_active_credentials(
        keycloak_base_url,
        issuer_service_client_id,
        issuer_service_client_secret,
        issuer_service_base_url,
    )

    # Filter credentials based on expiry date and validity
    expiring_credentials = filter_credentials(active_credentials, start_date, end_date)

    # Exit if there's nothing to reissue
    if not expiring_credentials:
        logging.warning("No expiring credentials found. Stopping execution.")
        return
    logging.info(
        f"Found {len(expiring_credentials)} active, valid credentials that expire between {start_date} and {end_date}.",
    )

    # Get operation IDs
    operation_ids: list[dict] = get_operation_ids(sap_auth_url, sap_client_id, sap_client_secret, sap_url)
    if not operation_ids:
        logging.error("No operation IDs found. Stopping execution.")
        return
    logging.info(f"Found {len(operation_ids)} operation IDs.")

    # Step 3: Merge data
    expiring_credential_data: list[dict] = [transform_credential_data(cred, stage) for cred in expiring_credentials]
    merged_credential_data = add_operation_id_to_credential_data(stage, sap_auth_url, sap_client_id, sap_client_secret,
                                                                 sap_url, expiring_credential_data, operation_ids)
    if not merged_credential_data:
        logging.error("No credential with valid operation ID after merging. Stopping execution.")
        return
    logging.info(f"Successfully merged {len(merged_credential_data)} records.")

    # Iterate over credentials
    iter_count = 0
    num_credentials_reissued = 0
    for cred in merged_credential_data:
        # Stop if at limit.
        if iter_limit is not None and iter_count >= int(iter_limit):
            logging.info(f"Reached processing limit of {iter_limit} credentials. Stopping execution.")
            break

        # Step 3: Merge data
        # Extract keys from credential data
        try:
            bpn: str = cred[KEY_BPN]
            cred_type: str = cred[KEY_TYPE]
            holder_did: str = cred[KEY_HOLDER_DID]
            credential_id: str = cred[KEY_CREDENTIAL_ID]
            operation_id: str = cred[KEY_OPERATION_ID]
        except KeyError as e:
            logging.error(f"Missing key in credential data:\n{cred}")
            raise e

        allowed_credential_types = [ct.value for ct in CredentialType]
        if cred_type not in allowed_credential_types:
            raise ValueError(f"Invalid credential type: {cred_type}.\nFull credential data: {cred}")

        # Get SAP client info
        customer_client_info: dict = get_customer_client_info(
            sap_auth_url,
            sap_client_id,
            sap_client_secret,
            sap_url,
            operation_id
        )
        wallet_url: str = customer_client_info[URL]
        tech_user_client_id: str = customer_client_info[CLIENT_ID]
        tech_user_client_secret: str = customer_client_info[CLIENT_SECRET]

        # Revoke old credential
        revoke_credential(
            keycloak_base_url,
            issuer_service_client_id,
            issuer_service_client_secret,
            issuer_service_base_url,
            credential_id
        )
        logger.info(f"Successfully revoked credential {credential_id}")

        # Issue new credential
        issue_credential(
            keycloak_base_url,
            issuer_service_client_id,
            issuer_service_client_secret,
            issuer_service_base_url,
            cred_type,
            holder_did,
            bpn,
            wallet_url,
            tech_user_client_id,
            tech_user_client_secret,
        )

        logger.info(f"Successfully requested reissued credential for BPN: {bpn} {cred_type}")
        num_credentials_reissued += 1
        iter_count += 1

    logger.info("=== Execution Summary ===")
    logger.info(f"Credentials reissued: {num_credentials_reissued}")
    logger.info("=====================")


if __name__ == "__main__":
    main()
