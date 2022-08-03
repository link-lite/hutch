import { useBackendApi } from "contexts/BackendApi";

import useSWR from "swr";

export const fetchKeys = {
  resultsModifierList: "ResultsModifiers",
  modifierTypeList: "ResultsModifiers/Types",
};

export const getResultsModifierApi = ({ api }) => ({
  create: ({ values }) =>
    api.post("ResultsModifiers", {
      json: values,
    }),
  update: ({ values, id }) =>
    api.put(`ResultsModifiers/${id}`, {
      json: values,
    }),
  delete: ({ values, id }) =>
    api.delete(`ResultsModifiers/${id}`, {
      json: values,
    }),
});

export const useResultsModifierList = () => {
  const { apiFetcher } = useBackendApi();
  return useSWR(fetchKeys.resultsModifierList, apiFetcher, { suspense: true });
};

export const useModifierTypeList = () => {
  const { apiFetcher } = useBackendApi();
  return useSWR(fetchKeys.modifierTypeList, apiFetcher, { suspense: true });
};
