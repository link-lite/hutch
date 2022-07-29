import logging
import os
import dotenv
import requests, requests.exceptions as req_exc
from typing import Any, Union


dotenv.load_dotenv()


def get_results_modifiers(activity_source_id: int) -> list:
    """Get the results modifiers for a given activity source.
    TODO: Once it is possible to query the results modifier endpoint
    by acitivity source ID, this function send a GET request to
    retrieve the relevant modifiers and return them as a list.

    Args:
        activity_source_id (int): The acivity source ID.

    Returns:
        list: The modifiers for the given activity source.
    """
    pass


def low_number_suppression(value: Union[int, float], threshold: int = 10) -> Union[int, float]:
    """Suppress values that fall below a given threshold.

    Args:
        value (Union[int, float]): The value to evaluate.
        threshold (int): The threshold to beat.

    Returns:
        Union[int, float]: `value` if `value` > `threshold` else `0`.
    """
    logger = logging.getLogger(os.getenv("DB_LOGGER_NAME"))
    logger.info("Applying Low Number Suppression.")
    result = value if value > threshold else 0
    logger.info(f"The count is {result} after Low Number Suppression.")
    return result


def apply_filters(value: Union[int, float], filters: list) -> Union[int, float]:
    """_summary_
    TODO: When more modifiers have been designed, use this function as a
    wrapper to iterate over the list of filters from `get_results_modifiers`,
    appyling them to the value until either the end last of the filters, or
    the value reaches 0.

    Args:
        value (Union[int, float]): _description_
        filters (list): _description_

    Returns:
        Union[int, float]: _description_
    """
    pass
