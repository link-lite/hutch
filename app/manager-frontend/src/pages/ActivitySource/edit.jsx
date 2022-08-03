import React from "react";
import { useBackendApi } from "contexts/BackendApi";
import { ActivitySource } from ".";
import { useParams } from "react-router-dom";
import { useActivitySource } from "api/activitysources";

function edit() {
  const {
    activitysource: { update },
  } = useBackendApi();
  const { id } = useParams();
  const { data } = useActivitySource(id);
  return <ActivitySource action={update} id={id} activitySource={data} />;
}

export default edit;
