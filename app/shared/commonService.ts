import { ConstantsMaster } from "./constants/constantsMaster";
import { PlanningCardModel } from "./interfaces/planningCardModel.interface";
import { CommitmentType as CommitmentTypeCodeEnum } from "src/app/shared/constants/enumMaster";
import { SortRow } from "./interfaces/sort-row.interface";
import { DateService } from "./dateService";
import { ValidationService } from "./validationService";
import { Office } from "./interfaces/office.interface";
import html2canvas from "html2canvas";
import jsPDF from "jspdf";
import { ResourceStaffing } from "./interfaces/resourceStaffing.interface";
import { ResourceAllocation } from "./interfaces/resourceAllocation.interface";
import { CaseType } from '../shared/constants/enumMaster';

export class CommonService {


  public static generate_UUID() {
    let date = new Date().getTime();
    const uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
      const randomNumber = (date + Math.random() * 16) % 16 || 0;
      date = Math.floor(date / 16);
      return (c === 'x' ? randomNumber : (randomNumber && 0x3 || 0x8)).toString(16);
    });
    return uuid;
  }

  public static hasAccessToFeature(featureName, accessibleFeatures) {
    if (Array.isArray(featureName)) {
      return accessibleFeatures?.some(x => featureName.some(f => x === f));
    }
    return accessibleFeatures?.some(x => x === featureName);
  }

  public static isAccessToFeatureReadOnlyOrNone(featureName, accessibleFeaturesForUser) {
    const isAccessable = !this.isReadOnlyAccessToFeature(featureName, accessibleFeaturesForUser) && !this.isLinkDisabledForFeature(featureName, accessibleFeaturesForUser);
    return isAccessable;
  }

  public static isReadOnlyAccessToFeature(featureName, accessibleFeatures) {
    if (Array.isArray(featureName)) {
      return accessibleFeatures.filter(x => featureName.includes(x.FeatureName))?.every(y => y.AccessTypeName === 'Read');
    }
    return accessibleFeatures.filter(x => x.FeatureName === featureName)?.every(y => y.AccessTypeName === 'Read');
  }

  public static isLinkDisabledForFeature(featureName, accessibleFeatures) {
    if (Array.isArray(featureName)) {
      return accessibleFeatures.filter(x => featureName.includes(x.FeatureName))?.every(y => y.AccessTypeName === 'None');
    }
    return accessibleFeatures.filter(x => x.FeatureName === featureName)?.every(y => y.AccessTypeName === 'None');
  }

  public static getAccessibleReports(accessiblePages) {
    let accessibleReports = [];
    accessiblePages.forEach(element => {
      if (element.trim().includes(ConstantsMaster.appScreens.page.analytics)) {
        accessibleReports.push(element.trim());
      }
    });
    return accessibleReports
  }

  public static getUniqueArrayOfObjects(arr, param1, param2) {
    let uniqueArray = [];
    arr.forEach(item => {
      let itemsFound = arr.filter(x => x[param1] === item[param1] && x[param2] === item[param2]);
      if (uniqueArray.indexOf(itemsFound[0]) < 0) {
        uniqueArray.push(itemsFound[0]);
      }
    });
    return uniqueArray;
  }

  public static findDuplicatesInArray(arr) {
    let sorted_arr = arr.slice().sort();
    let results = [];
    for (let i = 0; i < sorted_arr.length - 1; i++) {
      if (sorted_arr[i + 1] == sorted_arr[i]) {
        results.push(sorted_arr[i]);
      }
    }
    return results;
  }

  public static getAccessibleCommitmentTypes(claims) {
    let accessibleCommitmentTypes: string[] = [];
    let commitmentTypes = claims?.FeatureAccess?.filter(x => x.FeatureName.startsWith('commitmentType/')
      && (x.AccessTypeName === 'Read' || x.AccessTypeName === 'Write'));

    commitmentTypes.forEach(element => {
      accessibleCommitmentTypes.push(element.FeatureName.substring('15'))
    });
    return accessibleCommitmentTypes;

  }

  public static getEditableCommitmentTypes(claims, commitmentTypes) {
    let editableCommitmentTypeCodes: string[] = [];
    editableCommitmentTypeCodes = this.getEditableCommitmentTypesCodesList(claims);

    let editableCommitmentTypes = commitmentTypes.filter(x => editableCommitmentTypeCodes?.includes(x.commitmentTypeCode));
    const dummyCategory = { commitmentTypeCode: '', commitmentTypeName: 'Select Type', precedence: 0, isStaffingTag: false };
    editableCommitmentTypes.splice(0, 0, dummyCategory);
    return editableCommitmentTypes;

  }

  public static getEditableCommitmentTypesCodesList(claims) {
    let editableCommitmentTypeCodes: string[] = [];
    let editableCommitmentTypesFeature = claims?.FeatureAccess?.filter(x => x.FeatureName.startsWith('commitmentType/')
      && x.AccessTypeName === 'Write');

    editableCommitmentTypesFeature.forEach(element => {
      editableCommitmentTypeCodes.push(element.FeatureName.substring('15'))
    });

    return editableCommitmentTypeCodes;

  }

  public  static updateResourceAllocations(resourceAllocations: ResourceAllocation[], lastUpdatedBy: string) {

    resourceAllocations.forEach(resource => {
      resource.lastUpdatedBy = lastUpdatedBy; 
    
      if (resource.caseTypeCode && resource.caseTypeCode.toString() === CaseType.ClientDevelopment && !resource.investmentCode) {
        resource.investmentCode = ConstantsMaster.InvestmentCategory.AdHoc.investmentCode;
        resource.investmentName = ConstantsMaster.InvestmentCategory.AdHoc.investmentName;
      }
    });

    return resourceAllocations;
  }
  

  public static sortByDate(previousDate, nextDate, sortDirection = 'asc') {
    const higherComparaterValue = sortDirection === 'asc' ? 1 : -1;
    const lowerComparaterValue = sortDirection === 'asc' ? -1 : 1;
    let sortValue = 0;

    if (!previousDate)
      sortValue = higherComparaterValue;
    else if (!nextDate)
      sortValue = lowerComparaterValue;
    else if (new Date(previousDate).getTime() > new Date(nextDate).getTime())
      sortValue = higherComparaterValue;
    else if (new Date(previousDate).getTime() < new Date(nextDate).getTime())
      sortValue = lowerComparaterValue;
    return sortValue;
  }

  public static sortByString(previousElement, nextElement, sortDirection = 'asc') {
    const higherComparaterValue = sortDirection === 'asc' ? 1 : -1;
    const lowerComparaterValue = sortDirection === 'asc' ? -1 : 1;
    let sortValue = 0;

    if (!previousElement)
      sortValue = higherComparaterValue;
    else if (!nextElement)
      sortValue = lowerComparaterValue;
    else if (previousElement > nextElement)
      sortValue = higherComparaterValue;
    else if (previousElement < nextElement)
      sortValue = lowerComparaterValue;

    return sortValue;
  }

  public static sortByNumber(previousElement, nextElement, sortDirection = 'asc') {
    const higherComparaterValue = sortDirection === 'asc' ? 1 : -1;
    const lowerComparaterValue = sortDirection === 'asc' ? -1 : 1;
    let sortValue = 0;

    if (!previousElement)
      sortValue = higherComparaterValue;
    else if (!nextElement)
      sortValue = lowerComparaterValue;
    sortValue = previousElement - nextElement;

    return sortValue;
  }

  public static toggleClass(elements: HTMLElement[], className, isApply = false) {
    if (isApply) {
      elements.forEach(element => {
        if (element && !element.classList.contains(className))
          element.classList.add(className);
      });
    }
    else {
      elements.forEach(element => {
        if (element && element.classList.contains(className))
          element.classList.remove(className);
      });
    }
  }

  public static arrayMove(arr, fromIndex, toIndex) {
    var element = arr[fromIndex];
    arr.splice(fromIndex, 1);
    arr.splice(toIndex, 0, element);

    return arr;
  }

  public static isArraysEqual(arr1, arr2) {
    let arr1Length = arr1.length;
    let arr2Length = arr2.length;

    // If lengths of array are not equal means
    // array are not equal
    if (arr1Length != arr2Length)
      return false;

    // Sort both arrays
    arr1.sort();
    arr2.sort();

    // Linearly compare elements
    for (let i = 0; i < arr1Length; i++)
      if (arr1[i] != arr2[i])
        return false;

    // If all elements were same.
    return true;
  }

  public static ConvertToPlanningCardViewModel(projects, officeFlatList, staffingTagsList) {
    let planningCardModels: PlanningCardModel[] = [];

    projects.forEach((planningCard) => {
      const planningCardModel: PlanningCardModel = {
        id: planningCard.id,
        name: planningCard.name,
        projectName: planningCard.name,
        startDate: planningCard.startDate,
        endDate: planningCard.endDate,
        office: planningCard.office,
        isShared: planningCard.isShared,
        sharedOfficeCodes: planningCard.sharedOfficeCodes,
        sharedOfficeNames: CommonService.GetOfficeNames(planningCard.sharedOfficeCodes, officeFlatList),
        sharedStaffingTags: planningCard.sharedStaffingTags,
        sharedStaffingTagNames: CommonService.GetServiceLineNames(planningCard.sharedStaffingTags, staffingTagsList),
      };

      planningCardModels.push(planningCardModel);
    });

    return planningCardModels;
  }

  public static GetOfficeNames(sharedOfficeCodes, officeFlatList) {
    let sharedOffices = new Array();
    let sharedOfficeCodeList = sharedOfficeCodes.split(',');
    sharedOfficeCodeList.forEach(sharedOfficeCode => {
      sharedOffices.push((officeFlatList.find((x) => x.officeCode == sharedOfficeCode))?.officeAbbreviation)
    });

    return sharedOffices.join(', ');
  }

  public static GetServiceLineNames(sharedStaffingTags, staffingTagsList) {
    let sharedServiceLines = new Array();
    let sharedStaffingTagsList = sharedStaffingTags.split(',');
    sharedStaffingTagsList.forEach(sharedstaffingTag => {
      sharedServiceLines.push((staffingTagsList.find((x) => x.serviceLineCode == sharedstaffingTag))?.serviceLineName)
    });

    return sharedServiceLines.join(', ');
  }

  public static getResourcesSortAndGroupBySelectedValues(resources, sortBy, sortDirection = 'asc') {

    const sortByList = sortBy?.length > 1 ? sortBy.split(',') : null;
    const higherComparaterValue = sortDirection === 'asc' ? 1 : -1;
    const lowerComparaterValue = sortDirection === 'asc' ? -1 : 1;

    if (sortByList && resources?.length > 0) {
      resources.sort((previousElement, nextElement) => {
        for (let index = 0; index < sortByList.length; index++) {
          switch (sortByList[index]) {
            case 'levelGrade': {
              const comparer = this.sortAlphanumeric(previousElement.resource.levelGrade, nextElement.resource.levelGrade, sortDirection);
              if (comparer === 1 || comparer === -1) { return comparer; }
              break;
            }
            case 'office': {
              if (previousElement.resource.schedulingOffice.officeName > nextElement.resource.schedulingOffice.officeName) {
                return higherComparaterValue;
              }
              if (previousElement.resource.schedulingOffice.officeName < nextElement.resource.schedulingOffice.officeName) {
                return lowerComparaterValue;
              }
              break;
            }
            case 'position': {
              if (previousElement.resource[sortByList[index]].positionGroupName > nextElement.resource[sortByList[index]].positionGroupName) {
                return higherComparaterValue;
              }
              if (previousElement.resource[sortByList[index]].positionGroupName < nextElement.resource[sortByList[index]].positionGroupName) {
                return lowerComparaterValue;
              }
              break;
            }
            case 'dateFirstAvailable' || 'startDate': {
              //this is done to handle nulls as we want to push them down
              if (!previousElement.resource[sortByList[index]]) {
                return 1;
              }
              if (!nextElement.resource[sortByList[index]]) {
                return -1;
              }

              if (new Date(previousElement.resource[sortByList[index]]) > new Date(nextElement.resource[sortByList[index]])) {
                return higherComparaterValue;
              }
              if (new Date(previousElement.resource[sortByList[index]]) < new Date(nextElement.resource[sortByList[index]])) {
                return lowerComparaterValue;
              }
              break;
            }

            case 'lastBillableDate': {
              //this is done to handle nulls as we want to push them down
              if (!previousElement.resource.lastBillable?.lastBillableDate) {
                return 1;
              }
              if (!nextElement.resource.lastBillable?.lastBillableDate) {
                return -1;
              }

              if (new Date(previousElement.resource.lastBillable?.lastBillableDate) > new Date(nextElement.resource.lastBillable?.lastBillableDate)) {
                return higherComparaterValue;
              }
              if (new Date(previousElement.resource.lastBillable?.lastBillableDate) < new Date(nextElement.resource.lastBillable?.lastBillableDate)) {
                return lowerComparaterValue;
              }
              break;
            }
            case 'percentAvailable': {
              //this is done to handle nulls as we want to push them down
              if (!previousElement.resource[sortByList[index]]) {
                return 1;
              }
              if (!nextElement.resource[sortByList[index]]) {
                return -1;
              }

              if (previousElement.resource[sortByList[index]] > nextElement.resource[sortByList[index]]) {
                return higherComparaterValue;
              }
              if (previousElement.resource[sortByList[index]] < nextElement.resource[sortByList[index]]) {
                return lowerComparaterValue;
              }

              break;
            }
            default: {
              if (previousElement.resource[sortByList[index]]?.toLowerCase() > nextElement.resource[sortByList[index]]?.toLowerCase()) {
                return higherComparaterValue;
              }
              if (previousElement.resource[sortByList[index]]?.toLowerCase() < nextElement.resource[sortByList[index]]?.toLowerCase()) {
                return lowerComparaterValue;
              }
              break;
            }
          }


        }
      });
    }

    return resources;
  }

  public static sendEmailToSelectedResources(employees)
  {
    if (employees.length > 0) {
      let resourceEmails = employees
          .map(resource => resource.internetAddress)
          .filter(internetAddress => internetAddress);

      let emailBatches = [];
      let maxUrlLength = 2000; // Approximate safe URL length limit
      let baseMailto = "mailto:";
      let currentBatch = [];

      // Dynamically split emails into safe-sized batches
      resourceEmails.forEach(email => {
          let testBatch = [...currentBatch, email];
          let testMailto = baseMailto + testBatch.join(';');

          if (testMailto.length < maxUrlLength) {
              currentBatch.push(email);
          } else {
              emailBatches.push(currentBatch.join(';'));
              currentBatch = [email]; // Start a new batch
          }
      });

      if (currentBatch.length > 0) {
          emailBatches.push(currentBatch.join(';'));
      }

      // Function to open each batch with a delay
      let delay = 5000; // 5 second delay between openings
      emailBatches.forEach((batch, index) => {
          setTimeout(() => {
              window.location.href = `mailto:${batch}`;
          }, index * delay);
      });
  }

  }


  public static getResourcesSortBySelectedValues(resources, sortByRows: SortRow[], officeList: Office[]) {
    let sortByList = [];
    let sortByOfficeClusterData = sortByRows?.filter(x => x.field == 'officeCluster');
    let isSortByOfficeSelected = sortByRows?.some(x => x.field === 'office');
    if (!sortByRows || sortByRows.length === 0) {
      sortByList = sortByList.concat({ field: 'fullName', direction: 'asc' } as SortRow);
    }
    else {
      sortByList = sortByList.concat(sortByRows)
    }

    if(sortByOfficeClusterData && sortByOfficeClusterData.length){
      if(isSortByOfficeSelected){
        const sortByOfficeIndex = sortByList.findIndex(item => item.field === 'office');
        const sortByOfficeData = sortByList.splice(sortByOfficeIndex, 1);
        sortByList.push(sortByOfficeData[0]);

      }
      else{
        sortByList = sortByList.concat({ field: 'office', direction: sortByOfficeClusterData[0].direction } as SortRow);
      }
    }


    if (sortByList && resources?.length > 0) {
      resources.sort((previousElement, nextElement) => {

        for (let index = 0; index < sortByList.length; index++) {
          const higherComparaterValue = sortByList[index].direction === 'asc' ? 1 : -1;
          const lowerComparaterValue = sortByList[index].direction === 'asc' ? -1 : 1;

          switch (sortByList[index].field) {
            case 'levelGrade': {
              const comparer = this.sortAlphanumeric(previousElement.resource.levelGrade, nextElement.resource.levelGrade, sortByList[index].direction);
              if (comparer === 1 || comparer === -1) { return comparer; }
              break;
            }
            case 'office': {
              if (previousElement.resource.schedulingOffice.officeName > nextElement.resource.schedulingOffice.officeName) {
                return higherComparaterValue;
              }
              if (previousElement.resource.schedulingOffice.officeName < nextElement.resource.schedulingOffice.officeName) {
                return lowerComparaterValue;
              }
              break;
            }
            case 'position': {
              if (previousElement.resource[sortByList[index].field].positionGroupName > nextElement.resource[sortByList[index].field].positionGroupName) {
                return higherComparaterValue;
              }
              if (previousElement.resource[sortByList[index].field].positionGroupName < nextElement.resource[sortByList[index].field].positionGroupName) {
                return lowerComparaterValue;
              }
              break;
            }
            case 'dateFirstAvailable':
            case 'startDate': {
              //this is done to handle nulls as we want to push them down
              if (!previousElement.resource[sortByList[index].field]) {
                return 1;
              }
              if (!nextElement.resource[sortByList[index].field]) {
                return -1;
              }

              if (new Date(previousElement.resource[sortByList[index].field]) > new Date(nextElement.resource[sortByList[index].field])) {
                return higherComparaterValue;
              }
              if (new Date(previousElement.resource[sortByList[index].field]) < new Date(nextElement.resource[sortByList[index].field])) {
                return lowerComparaterValue;
              }
              break;
            }

            case 'commitmentStartDate': {

              let previousElementCommitments = this.getAllResourceCommitmentsSortedBasedOnStartDate(previousElement);
              let nextElementCommitments = this.getAllResourceCommitmentsSortedBasedOnStartDate(nextElement);
              if (!previousElementCommitments || previousElementCommitments.length === 0) {
                return higherComparaterValue;
              }
              if (!nextElementCommitments || nextElementCommitments.length === 0) {
                return lowerComparaterValue;
              }

              if (new Date(previousElementCommitments[0].startDate) > new Date(nextElementCommitments[0].startDate)) {
                return higherComparaterValue;
              }
              if (new Date(previousElementCommitments[0].startDate) < new Date(nextElementCommitments[0].startDate)) {
                return lowerComparaterValue;
              }
              break;
            }


            case 'lastBillableDate': {
              //this is done to handle nulls as we want to push them down
              if (!previousElement.resource.lastBillable?.lastBillableDate) {
                return 1;
              }
              if (!nextElement.resource.lastBillable?.lastBillableDate) {
                return -1;
              }

              if (new Date(previousElement.resource.lastBillable?.lastBillableDate) > new Date(nextElement.resource.lastBillable?.lastBillableDate)) {
                return higherComparaterValue;
              }
              if (new Date(previousElement.resource.lastBillable?.lastBillableDate) < new Date(nextElement.resource.lastBillable?.lastBillableDate)) {
                return lowerComparaterValue;
              }
              break;
            }
            case 'percentAvailable': {
              //this is done to handle nulls as we want to push them down
              if (!previousElement.resource[sortByList[index].field]) {
                return 1;
              }
              if (!nextElement.resource[sortByList[index].field]) {
                return -1;
              }

              if (previousElement.resource[sortByList[index].field] > nextElement.resource[sortByList[index].field]) {
                return higherComparaterValue;
              }
              if (previousElement.resource[sortByList[index].field] < nextElement.resource[sortByList[index].field]) {
                return lowerComparaterValue;
              }

              break;
            }

            case 'officeCluster':{

              var previousOfficeCluster = officeList.find(x=>x.officeCode == previousElement.resource.schedulingOffice.officeCode)?.officeCluster;
              var nextOfficeCluster = officeList.find(x=>x.officeCode == nextElement.resource.schedulingOffice.officeCode)?.officeCluster;

              if(previousOfficeCluster > nextOfficeCluster){
                return higherComparaterValue;
              }
              else if(previousOfficeCluster < nextOfficeCluster){
                return lowerComparaterValue;
              }
             
            }

            default: {
              if (previousElement.resource[sortByList[index].field]?.toLowerCase() > nextElement.resource[sortByList[index].field]?.toLowerCase()) {
                return higherComparaterValue;
              }
              if (previousElement.resource[sortByList[index].field]?.toLowerCase() < nextElement.resource[sortByList[index].field]?.toLowerCase()) {
                return lowerComparaterValue;
              }
              break;
            }
          }


        }
      });
    }

    return resources;
  }

  public static getAllResourceCommitmentsSortedBasedOnStartDate(resourceData: ResourceStaffing) {
    let commitmentArray: any[] = [];
  
    if (resourceData.loAs.length > 0) {
      commitmentArray.push(...resourceData.loAs);
    }
  
    if (resourceData.vacations.length > 0) {
      commitmentArray.push(...resourceData.vacations);
    }
  
    if (resourceData.timeOffs.length > 0) {
      commitmentArray.push(...resourceData.timeOffs);
    }
  
     
  if (resourceData.commitments.length > 0) {

    let loAs = this.getCommitmentArrayExistsForResource(resourceData, CommitmentTypeCodeEnum.LOA);
    if (loAs.length > 0) {
      commitmentArray.push(...loAs);
    }

    let vacations = this.getCommitmentArrayExistsForResource(resourceData, CommitmentTypeCodeEnum.VACATION);
    if (vacations.length > 0) {
      commitmentArray.push(...vacations);
    }

    let trainings = this.getCommitmentArrayExistsForResource(resourceData, CommitmentTypeCodeEnum.TRAINING);
    if (trainings.length > 0) {
      commitmentArray.push(...trainings);
    }

    let recruitings = this.getCommitmentArrayExistsForResource(resourceData, CommitmentTypeCodeEnum.RECRUITING);
    if (recruitings.length > 0) {
      commitmentArray.push(...recruitings);
    }

    let shortTermAvailable = this.getCommitmentArrayExistsForResource(resourceData, CommitmentTypeCodeEnum.SHORT_TERM_AVAILABLE);
    if (shortTermAvailable.length > 0) {
      commitmentArray.push(...shortTermAvailable);
    }

    let notAvailable = this.getCommitmentArrayExistsForResource(resourceData, CommitmentTypeCodeEnum.NOT_AVAILABLE);
    if (notAvailable.length > 0) {
      commitmentArray.push(...notAvailable);
    }

    let limitedAvailability = this.getCommitmentArrayExistsForResource(resourceData, CommitmentTypeCodeEnum.LIMITED_AVAILABILITY);
    if (limitedAvailability.length > 0) {
      commitmentArray.push(...limitedAvailability);
    }

    let downDay = this.getCommitmentArrayExistsForResource(resourceData, CommitmentTypeCodeEnum.DOWN_DAY);
      if (downDay.length > 0) {
        commitmentArray.push(...downDay);
      }
    }
  
    if (resourceData.trainings.length > 0) {
      commitmentArray.push(...resourceData.trainings);
    }
  
    if (resourceData.holidays.length > 0) {
      commitmentArray.push(...resourceData.holidays);
    }

  
    // Sort by startDate in ascending order
    commitmentArray.sort((a, b) => {
      return new Date(a.startDate).getTime() - new Date(b.startDate).getTime();
    });
  
    return commitmentArray;
  }

  public static getCommitmentArrayExistsForResource(resourceData: ResourceStaffing, commitmentTypeCode) {
    return resourceData.commitments.filter(x => x.commitmentTypeCode === commitmentTypeCode);
  }

  public static sortAlphanumeric(previous, next, sortDirection) {
    const regexAlpha = /[^a-zA-Z]/g;
    const regexNumeric = /[^0-9]/g;
    const previousAlphaPart = previous.replace(regexAlpha, '');
    const nextAlphaPart = next.replace(regexAlpha, '');
    const higherComparaterValue = sortDirection === 'asc' ? 1 : -1;
    const lowerComparaterValue = sortDirection === 'asc' ? -1 : 1;

    if (previousAlphaPart === nextAlphaPart) {
      const previousNumericPart = parseInt(previous.replace(regexNumeric, ''), 10);
      const nextNumericPart = parseInt(next.replace(regexNumeric, ''), 10);
      if (previousNumericPart > nextNumericPart) { return higherComparaterValue; }
      if (previousNumericPart < nextNumericPart) { return lowerComparaterValue; }
    } else {
      if (previousAlphaPart > nextAlphaPart) { return higherComparaterValue; }
      if (previousAlphaPart < nextAlphaPart) { return lowerComparaterValue; }
    }

  }

  public static isCommonElementsInTwoCommaSeparatedStrings(str1, str2) {
    const str1Array: string[] = !str1 ? [] : str1.split(",");
    const str2Array: string[] = !str2 ? [] : str2.split(",");

    const commonValues: string[] = str1Array.filter(value => str2Array.includes(value));

    if (commonValues.length > 0) {
      return true;
    } else {
      return false;
    }

  }

  public static getCommitmentColorClass(commitmentTypeCode, investmentCode, pipelineId,oldCaseCode,isIncludeInCapacityReporting,vacationStatus): string {
    let colorClass = "";

    switch (commitmentTypeCode) {
      case CommitmentTypeCodeEnum.CASE_OPP: {
        if(pipelineId && !oldCaseCode){
          colorClass = "yellow-opp";
        }
        else{
          switch (investmentCode) {
            case ConstantsMaster.InvestmentCategory.InternalPD.investmentCode: {
              colorClass = "orange";
              break;
            }
            case ConstantsMaster.InvestmentCategory.ClientVariable.investmentCode: {
              colorClass = "teal";
              break;
            }
            case ConstantsMaster.InvestmentCategory.PrePostRev.investmentCode: {
              colorClass = "grey";
              break;
            }
            default: {
              colorClass = "blue";
              break;
            }
          }
        }
        break;
      }
      case CommitmentTypeCodeEnum.NAMED_PLACEHOLDER:
      case CommitmentTypeCodeEnum.PLANNING_CARD:{
        if(isIncludeInCapacityReporting)
          colorClass = "red-pc";
        else
          colorClass = "green-pc";
        break;
      }
      case CommitmentTypeCodeEnum.LOA: {
        colorClass = "pink";
        break;
      }
      case CommitmentTypeCodeEnum.HOLIDAY:
      case CommitmentTypeCodeEnum.AAG:
      case CommitmentTypeCodeEnum.ADAPT:
      case CommitmentTypeCodeEnum.FRWD: {
        colorClass = "purple";
        break;
      }
      case CommitmentTypeCodeEnum.VACATION : {
        if(vacationStatus == 'Submitted'){
          colorClass = "light-purple";
        }
        else{
          colorClass = "purple";
        }
        break;
      }
      case CommitmentTypeCodeEnum.TRAINING: {
        colorClass = "light-red";
        break;
      }
      case CommitmentTypeCodeEnum.RECRUITING: {
        colorClass = "yellow";
        break;
      }
      case CommitmentTypeCodeEnum.SHORT_TERM_AVAILABLE:
      case CommitmentTypeCodeEnum.NOT_AVAILABLE:
      case CommitmentTypeCodeEnum.LIMITED_AVAILABILITY: {
        colorClass = "green";
        break;
      }
      case CommitmentTypeCodeEnum.PEG:
      case CommitmentTypeCodeEnum.PEG_Surge: {
        colorClass = "lavender";
        break;
      }
      case CommitmentTypeCodeEnum.DOWN_DAY: {
        colorClass = "blue-grey";
        break;
      }
      case CommitmentTypeCodeEnum.NOT_BILLABLE:{
        colorClass ="light-red";
        break;
      }
      default: {
        colorClass = "blue";
        break;
      }
    }

    return colorClass;
  }

  public static validateUserInputByOperatorAndType(data, filterValue , operator, type) {

    let isFiltered = false;
    switch (type) {
      case 'string':
        isFiltered = this.validateUserInputByOperatorForStringType(data, filterValue, operator);
        break;
      case 'date':
        isFiltered = this.validateUserInputByOperatorForDateType(data, filterValue, operator);
        break;
      case 'number':
        isFiltered = this.validateUserInputByOperatorForNumberType(data, filterValue, operator);
        break;
      case 'array':
        isFiltered = this.validateUserInputByOperatorForArrayType(data, filterValue, operator);
        break;
    }
    return isFiltered;
  }

  public static validateUserInputByOperatorForStringType(data, filterValue, operator: 'equals' | 'notEquals') {
    if(!data) {
      return false;
    }
    
    const castedData = String(data);
    const castedFilterValue = String(filterValue);

    let isFiltered = false;
    switch (operator) {
      case 'equals':
        isFiltered = castedData == castedFilterValue;
        break;
      case 'notEquals':
        isFiltered = castedData != castedFilterValue
        break;
    }
    return isFiltered;
  }

  public static validateUserInputByOperatorForArrayType(data, filterValue, operator: 'equals' | 'notEquals') {
    const castedData = String(data);
    const castedFilterValue = String(filterValue);

    let isFiltered = false;
    switch (operator) {
      case 'equals':
        isFiltered = this.isCommonElementsInTwoCommaSeparatedStrings(castedData, castedFilterValue);
        break;
      case 'notEquals':
        isFiltered = !this.isCommonElementsInTwoCommaSeparatedStrings(castedData, castedFilterValue);
        break;
    }
    return isFiltered;
  }

  public static validateUserInputByOperatorForNumberType(data, filterValue, operator: 'greaterThan' | 'lesserThan' | 'equals' | 'notEquals' | 'between') {
    if(ValidationService.isNullEmptyOrUndefined(data)) {
      return false;
    }

    const castedData = Number(data);
    const castedFilterValue = Number(filterValue);
    let isFiltered = false;

    switch (operator) {
      case 'greaterThan':
        isFiltered = castedData > castedFilterValue;
        break;
      case 'lesserThan':
        isFiltered = castedData < castedFilterValue;
        break;
      case 'equals':
        isFiltered = castedData === castedFilterValue;
        break;
      case 'notEquals':
        isFiltered = castedData !== castedFilterValue;
        break;
      case 'between':
        const rangeStart = Number(filterValue.split(',')[0]);
        const rangeEnd = Number(filterValue.split(',')[1]);

        isFiltered = castedData >= rangeStart && castedData <= rangeEnd;
        break;
    }
    return isFiltered;
  }

  public static validateUserInputByOperatorForDateType(data, filterValue,  operator: 'greaterThan' | 'lesserThan' | 'equals' | 'notEquals' | 'between') {
    if(!data) {
      return false;
    }

    const castedData = new Date(data);
    const castedFilterValue = new Date(filterValue);
    let isFiltered = false;

    switch (operator) {
      case 'greaterThan':
        isFiltered = DateService.isSameOrAfter(castedData, filterValue, true);
        break;
      case 'lesserThan':
        isFiltered = DateService.isSameOrBefore(castedData, filterValue, true);
        break;
      case 'equals':
        isFiltered = DateService.isSame(castedData, filterValue);
        break;
      case 'notEquals':
        isFiltered = DateService.isNotSame(castedData, filterValue);
        break;
      case 'between':
        const valueStart = filterValue.split(',')[0];
        const valueEnd = filterValue.split(',')[1];

        isFiltered = DateService.isSameOrAfter(castedData, valueStart) && DateService.isSameOrBefore(castedData, valueEnd );
        break;
    }
    return isFiltered;
  }

  public static showCompleteAllocationsList(originalStyles, listElement) {
    originalStyles.push({
      element: listElement,
      height: listElement.style.height,
      overflow: listElement.style.overflow
    });

    // Set the height to 100% to avoid scrolls
    listElement.style.height = '100%';
    listElement.style.height = 'auto';
    listElement.style.maxHeight = 'none';
    listElement.style.overflow = 'visible';
  }

  public static generatePdf(elementId: string, pdfFilename: string, convertToOriginalSize: boolean = false) {
    const element = document.getElementById(elementId);

    if (element) {
      // Store original styles of scrollable nested divs within the specified wrapper
      const originalStyles: { element: HTMLElement, height: string, overflow: string }[] = [];
  
      try {  
        const expandedWrappers = element.querySelectorAll('.pdfExportScrollableContainer');
        const probabilityPercentFields = element.querySelectorAll('.probability-percent-field');
  
        expandedWrappers.forEach(wrapper => {
            const cardElement = wrapper as HTMLElement;
            const allocationListElement = cardElement.querySelector('.allocation__list') as HTMLElement;
            const placeholderListElement = cardElement.querySelector('.placeholder__list') as HTMLElement;
            if (allocationListElement) {
              this.showCompleteAllocationsList(originalStyles, allocationListElement);
            }
            if(placeholderListElement) {
              this.showCompleteAllocationsList(originalStyles, placeholderListElement);
            }
        });

        probabilityPercentFields.forEach(field => {
          const probabilityPercentFieldElement = field as HTMLElement;
          originalStyles.push({
            element: probabilityPercentFieldElement,
            height: probabilityPercentFieldElement.style.height,
            overflow: probabilityPercentFieldElement.style.overflow
          });

          probabilityPercentFieldElement.style.verticalAlign = 'top';
        });

        // Ensure the element's parent container is large enough
        element.style.overflow = 'visible';
        element.style.height = 'auto';
  
        // Reset original styles of scrollable nested divs
        originalStyles.forEach(style => {
          style.element.style.height = style.height;
          style.element.style.overflow = style.overflow;
        });
  

        html2canvas(element).then(canvas => {
          const ganttWidth = canvas.width;
          const ganttHeight = canvas.height;
          const top_left_margin = 15;
          const page_bottom_margin = 50;
          const PDF_Width = ganttWidth + (top_left_margin * 2);
          const PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
          const canvas_image_width = ganttWidth;
          const canvas_image_height = ganttHeight - page_bottom_margin;
          const totalPDFPages = Math.ceil(ganttHeight / (PDF_Height - page_bottom_margin));
          
          const imgData = canvas.toDataURL("image/jpeg", 1.0);
          
          const pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
          pdf.addImage(imgData, 'JPG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
          
          for (let i = 1; i <= totalPDFPages; i++) {
              pdf.addPage();
              const yPos = -(PDF_Height * i) + (top_left_margin);
              pdf.addImage(imgData, 'JPEG', top_left_margin, yPos, canvas_image_width, canvas_image_height);
          }

          pdf.save(pdfFilename);

          if(!convertToOriginalSize){
            setTimeout(() => {
              window.close();
            }, 0);
          }

          if(convertToOriginalSize){
            // Restore original styles after PDF generation
            element.style.overflow = originalStyles[0].overflow;
            element.style.height = originalStyles[0].height;
          }
        }) 
    } catch(error) {
      console.error('Error capturing the element:', error);
    }
  } else {
      console.error(`Element with id '${elementId}' not found`);
    }
  }

}
