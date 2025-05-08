/// <reference lib="webworker" />

import { ResourceService } from "../helperServices/resource.service";

addEventListener('message', (event) => {
  const input = JSON.parse(event.data);
  const response = ResourceService.createResourcesDataForStaffing(input.resourcesData, input.searchStartDate, input.searchEndDate,
    input.supplyFilterCriteriaObj, input.commitmentTypes, input.userPreferences, input.isTriggeredFromSearch);
  postMessage(response);
});