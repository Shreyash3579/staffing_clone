import { DateService } from './dateService';
import * as moment from 'moment';

export class ValidationService {
  public static regexGUID = new RegExp('^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$');
  public static regexNumber = new RegExp('^[0-9]+$');
  public static regexDecimal = new RegExp('^\\d+(\\.\\d{0,2})?$');
  public static alphaCharsPattern = new RegExp(/^[0-9]+$/);
  public static specialCharsPattern = new RegExp(/[~`!#$\^&*+=\-\[\]\\';,/{}|\\":<>\?.]/);
  public static regexDate = new RegExp('^(([0-9])|([0-2][0-9])|([3][0-1]))\-(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)\-\+?\d{0,2}');
  public static requiredMessage = 'Please complete all required fields';
  public static dateInvalidMessage = 'Please enter a valid date \n(i.e.dd - mmm - yyyy)';
  public static minDateInvalidMessage = 'End date cannot be that far in the past';
  public static numberInvalidMessage = 'Please enter a valid number';
  public static positiveNumberMessage = 'Please enter a number greater than 0';
  public static typeaheadInvalidMessage = 'Invalid value';
  public static startDateGreaterThanEndDate = 'Start date cannot be greater than end date';
  public static endDateLesserThanStartDate = 'End date cannot be lesser than start date';
  public static dateSmallerThanProjectDate = 'End date cannot be smaller that the current Project end date';
  public static dateDiffInvalid = 'Allocation cannot be of more than one year';
  public static requiredFieldMessage = 'Required';
  public static invalidEmployee = 'Invalid employee';
  public static invalidAllocation = 'Please enter a valid percentage';
  public static dateOutsideDateRange = 'Date should be between allocation start and end date';
  public static probablePercentageLimitExceeds = 'Probability percentage cannot be greater than 100';
  public static terminatedEmployeeAllocation = 'Terminated employees cannot be allocated';
  public static terminatedEmployeeCommitment = 'Cannot add commitments for terminated employees';
  public static atleastOneAllocationSelected = 'Atleast one resource should be selected';
  public static addSTACommitmentFornoUpcomingAllocations = 'Short-Term Available commitment cannot be created without any upcoming allocations';
  public static caseRollToSameCase = 'Select another case';
  public static caseRollForNoActiveAllocationMessage = 'Cannot roll a case with no active allocations in past 30 days';
  public static caseRollNotAllowedForInActiveCasesMessage = 'Cannot roll a case that has been inactive for more than 30 days';
  public static editingNotAllowed = 'Editing not allowed.';
  public static invalidInvestmentTypeMessage1 = '"Pre/Post Revenue" cannot be selected for allocation within the case/opp date range';
  public static invalidInvestmentTypeMessage2 = 'Cannot change the investment type as allocation is not within case/opp date range';
  public static errorWhileLoading = 'Error while loading the data. Please retry';
  public static minDateYear = 1970;
  public static noResultsFoundMsg = 'No Results Found.';
  public static joiningDateGreaterThanEndDate = "Resource can not be assigned on Case before its joining Date";
  public static employeeJoiningDateGreaterThanStartDate = "Allocation start date cannot be before Employee's joining date '[joiningDate]'";
  public static joiningDateGreaterThanStartDate = "Joining Date can not be greater than start date";
  public static employeeMissingOnGuessedPlaceholder = "Allocate resource from supply using drag-drop before confirming placeholder"
  public static monthClosedForAllocationUpdates = "Cannot change the allocation data as Financials for [monthName] has been closed";
  public static invalidAdminUserMsg = "Please select valid user";
  public static selectedUserAlreadyExist = "Selected user already exist";
  public static sameBucketMovementMsg = "Moving cases between same bucket is currently not allowed !!";
  public static casePlanningBoardUpdatedSuccessfullyMsg = "Board updated succesfully";
  public static casePlanningBoardIncludeInDemandSuccessMsg = "Include in demand updated succesfully";
  public static projectExistsonBoard = "Case/Opportunity already exists on the board. Please add another case/opportuntiy";
  public static invalidWhiteboardIdMsg = "Whiteboard ID invalid. Please try again with a different ID.";
  public static sortByValidationMessage = "Please select Sort field!";
  public static filterByValidationMessage = "Please select filter field, operator and value!";
  public static skuSizeValidationMessage = "Team size must be between 2 and 6 roles for Cortex-generated workstreams.";

  public static isAllocationValid(allocation: any, isZeroAllowed = true) {
    if (allocation?.toString().trim().length < 1) {
      return false;
    } else if (parseInt(allocation, 10).toString().length > 3) {
      return false;
    } else if (isNaN(allocation)) {
      if (parseInt(allocation, 10) < 0 ||
        allocation?.split('%').length > 1 && allocation?.split('%')[1] !== '' ||
        !this.alphaCharsPattern.test(allocation) && allocation[allocation?.length - 1] !== '%' ||
        this.specialCharsPattern.test(allocation) ||
        allocation?.indexOf('.') > -1 && allocation !== Math.floor(allocation)
      ) {
        return false;
      }
      return true;
    } else if (allocation?.toString().indexOf('-') > -1 || allocation?.toString().indexOf('.') > -1) {
      return false;
    } else if (parseInt(allocation, 10) === 0 && !isZeroAllowed) {
      return false;
    } else {
      return true;
    }
  }

  public static isInvestmentTypeValid(resourceAllocation) {
    const obj = {
      isValid: true,
      errorMessage: ''
    };
    let projectStartDate = null;
    let projectEndDate = null;
    if (!!resourceAllocation.oldCaseCode && !!resourceAllocation.caseStartDate
      && !!resourceAllocation.caseEndDate) {
      projectStartDate = resourceAllocation.caseStartDate;
      projectEndDate = resourceAllocation.caseEndDate;
    } else if (!resourceAllocation.oldCaseCode && !!resourceAllocation.opportunityStartDate
      && !!resourceAllocation.opportunityEndDate) {
      projectStartDate = resourceAllocation.opportunityStartDate;
      projectEndDate = resourceAllocation.opportunityEndDate;
    }
    if (!!projectStartDate && !!projectEndDate) {
      if (moment(resourceAllocation.startDate).isSameOrAfter(moment(projectStartDate)) &&
        moment(resourceAllocation.endDate).isSameOrBefore((projectEndDate))) {
        // If allocation is within case daterange then investmentType should not be Pre-Post
        if (resourceAllocation.investmentCode === 4) {
          obj.isValid = false;
          obj.errorMessage = this.invalidInvestmentTypeMessage1;
        }
      } else if (resourceAllocation.investmentCode != 4) {
        obj.isValid = false;
        obj.errorMessage = this.invalidInvestmentTypeMessage2;
      }
    }
    return obj;
  }

  public static isValidGUID(value){
    if (!value || !this.regexGUID.test(value)) {
      return false;
    }

    return true;
  }

  public static isValidNumberBetween(value, start = 0, end = 100) {
    if (!this.regexNumber.test(value) || !(parseInt(value) >= start && parseInt(value) <= end)) {
      return false;
    }

    return true;
  }

  public static isValidDecimalBetween(value, start = 0, end = 100) {
    if (!this.regexDecimal.test(value) || !(parseInt(value) >= start && parseInt(value) <= end)) {
      return false;
    }

    return true;
  }

  public static invalidNumberErrorMessage(start = 0, end = 999) {
    return 'Number should be between ' + start + ' and ' + end;
  }

  public static isValidDate(value: string) {
    if (this.isStringNullOrEmpty(value) || !this.isDate(value)) {
      return false;
    }
    value = DateService.convertDateInBainFormat(value).toLocaleUpperCase();

    if (this.regexDate.test(value)) {
      const fullDate = value.split('-');
      const date = fullDate[0].trim();
      const month = fullDate[1].trim();
      const year = fullDate[2].trim();

      // Since regexDate only test the format and not the actual date
      if (!this.regexNumber.test(date) || year.length !== 4 || !this.regexNumber.test(year)) {
        return false;
      }

      const data = DateService.calendar.filter(function (value) {
        return value.monthShortName === month;
      });

      if (month === 'FEB' && DateService.isLeapYear(parseInt(year))) {
        if (parseInt(date) <= 0 || parseInt(date) > 29) {
          return false;
        }
      } else {
        if (parseInt(date) <= 0 || parseInt(date) > parseInt(data[0].enddate)) {
          return false;
        }
      }
      return true;
    }
    return false;
  }

  public static isDateRangeValid(startDate: Date, endDate: Date) {
    const dateDiffInDays = DateService.getDatesDifferenceInDays(startDate, endDate);
    if (DateService.isLeapYear() ? dateDiffInDays > DateService.daysInLeapYear : dateDiffInDays > DateService.daysInYear) {
      return false;
    }
    return true;
  }

  public static checkIfAllocationIsOfOneYear(startDate: Date, endDate: Date) {
    const dateDiffInDays = DateService.getDatesDifferenceInDays(startDate, endDate);
    if (DateService.isLeapYear(startDate.getFullYear())
      ? dateDiffInDays >= DateService.daysInLeapYear
      : dateDiffInDays >= DateService.daysInYear) {
      return true;
    }
    return false;
  }

  public static isDateValid(startDate: string, endDate: string) {
    startDate = startDate?.toLocaleUpperCase();
    endDate = endDate?.toLocaleUpperCase();
    if (this.regexDate.test(endDate)) {
      const fullDate = endDate?.split('-');
      const date = fullDate[0].trim();
      const month = fullDate[1].trim();
      // let year = fullDate[2].trim();

      const data = DateService.calendar.filter(function (value) {
        return value.monthShortName === month;
      });
      
      if(DateService.isLeapYear() && month === 'FEB') {
        data[0].enddate = '29';
      }

      if (parseInt(date, 10) <= 0 || parseInt(date, 10) > parseInt(data[0].enddate, 10)) {
        return this.dateInvalidMessage;
      } else if (Date.parse(endDate) < Date.parse(startDate)) {
        return this.startDateGreaterThanEndDate;
      }
      return '';
    }

    return this.dateInvalidMessage;
  }

  private static isDate(obj) {
    obj = new Date(obj);
    if (obj && typeof obj === 'object') {
      return !!(obj.getFullYear() && (obj.getMonth() + 1) && obj.getDate());
    } else {
      return false;
    }
  }

  public static validateDate(obj) {

    const validationObj = {
      isValid: true,
      errorMessage: ''
    };

    if (obj === null || obj === '' || !this.isDate(obj) || (typeof(obj) !== 'string' && isNaN(obj.getTime())) || this.isDateLessThanMinDateForBOSS(obj) ) {

      validationObj.isValid = false;
      validationObj.errorMessage = this.dateInvalidMessage;

    } else {

      validationObj.isValid = true;
      validationObj.errorMessage = '';

    }

    return validationObj;
  }

  public static isInteger(value: string) {
    if (this.regexNumber.test(value)) {
      return true;
    }
    return false;
  }

  public static isDecimal(value: string) {
    if (this.regexDecimal.test(value)) {
      return true;
    }
    return false;
  }

  public static validateDecimalBetween(number, start = 0, end = 100) {

    const errorObject = {
      isValid: true,
      errorMessage: ''
    };

    if (number === null || number === undefined || !this.isDecimal(number)) {
      errorObject.isValid = false;
      errorObject.errorMessage = this.invalidNumberErrorMessage(start, end);

    } else if (!this.isValidDecimalBetween(number, start, end)){
      errorObject.isValid = false;
      errorObject.errorMessage = this.invalidNumberErrorMessage(start, end);

    }else{
      errorObject.isValid = true;
      errorObject.errorMessage = '';
    }

    return errorObject;
  }

  public static isPositiveInteger(value: string) {
    if (this.isInteger(value) && parseInt(value, 10) > 0) {
      return true;
    } else {
      return false;
    }
  }

  public static validatePositiveInteger(number) {

    const errorObject = {
      isValid: true,
      errorMessage: ''
    };

    if (!number || !this.isInteger(number)) {
      errorObject.isValid = false;
      errorObject.errorMessage = this.numberInvalidMessage;

    } else if (!this.isPositiveInteger(number)) {
      errorObject.isValid = false;
      errorObject.errorMessage = this.positiveNumberMessage;

    } else {
      errorObject.isValid = true;
      errorObject.errorMessage = '';

    }

    return errorObject;
  }

  public static isRequired(value) {
    const errorObject = {
      isValid: true,
      errorMessage: ''
    };
    if (this.isStringNullOrEmpty(value)) {
      errorObject.isValid = false;
      errorObject.errorMessage = this.requiredFieldMessage;
    }
    return errorObject;
  }

  public static isStringNullOrEmpty(string: string) {
    return string === null || string === undefined || string === '';
  }

  public static isNullEmptyOrUndefined(value: any) {
    return value === null || value === undefined || value === '';
  }

  public static validateProbablePercentage(value) {
    const errorObject = {
      isValid: true,
      errorMessage: ''
    };
    if (this.isNullEmptyOrUndefined(value)) {
      errorObject.isValid = false;
      errorObject.errorMessage = this.requiredFieldMessage;
    }
    if (!this.isPositiveInteger(value)) {
      errorObject.isValid = false;
      errorObject.errorMessage = this.positiveNumberMessage;
    }
    if (this.isInteger(value) && parseInt(value, 10) > 100) {
      errorObject.isValid = false;
      errorObject.errorMessage = this.probablePercentageLimitExceeds;
    }
    return errorObject;
  }

  public static isCaseEligibleForRoll(caseEndDate) {
    return moment(caseEndDate).startOf('day') >= moment().subtract(30, 'days').startOf('day');
  }

  public static isDateLessThanMinDateForBOSS(date) {
    return (new Date(date).getFullYear() < this.minDateYear);
  }
}
