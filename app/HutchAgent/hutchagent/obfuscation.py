import logging
import os
import dotenv
import requests, requests.exceptions as req_exc
from typing import Any, Union


dotenv.load_dotenv()


def get_results_modifiers(activity_source_id: str) -> list:
    """Get the results modifiers for a given activity source.

    Args:
        activity_source_id (str): The acivity source ID.

    Returns:
        list: The modifiers for the given activity source.
    """
    logger = logging.getLogger(os.getenv("DB_LOGGER_NAME"))
    try:
        res = requests.get(f"{os.getenv('MANAGER_URL')}/api/resultsmodifiers")
        modifiers = res.json()
        logger.info(
            f"Retrieved {len(modifiers)} results modifiers for activity source {activity_source_id}"
        )
        return modifiers
    except req_exc.ConnectionError as connection_error:
        logger.error(str(connection_error))
    except req_exc.Timeout as timeout_error:
        logger.error(str(timeout_error))
    except req_exc.MissingSchema as missing_schema_error:
        logger.error(str(missing_schema_error))
    return list()


def low_number_suppression(value: Union[int, float], threshold: int = 10) -> Union[int, float]:
    """Suppress values that fall below a given threshold.

    Args:
        value (Union[int, float]): The value to evaluate.
        threshold (int): The threshold to beat.

    Returns:
        Union[int, float]: `value` if `value` > `threshold` else `0`.
    """
    return value if value > threshold else 0


def apply_filters(value: Union[int, float], filters: list) -> Union[int, float]:
    """_summary_

    Args:
        value (Union[int, float]): _description_
        filters (list): _description_

    Returns:
        Union[int, float]: _description_
    """
    filter_types = {
        "LowNumberSuppression": low_number_suppression
    }
    result = value
    for f in filters:
        if func := filter_types.get(f.Id):
            result = func(**f.Parameters)
    return result