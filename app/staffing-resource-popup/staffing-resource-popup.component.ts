import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import * as moment from "moment";

// interfaces
import { CommitmentType } from "../shared/interfaces/commitmentType.interface";

// constants
import { ConstantsMaster } from "../shared/constants/constantsMaster";
import { CommitmentType as CommitmentTypeEnum } from "../shared/constants/enumMaster";
import { CommitmentType as CommitmentTypeCodeEnum } from "../shared/constants/enumMaster";

// services
import { StaffingResourcePopupService } from "./staffing-resource-popup.service";
import { DateService } from "../shared/dateService";
import { CommonService } from "../shared/commonService";

@Component({
  selector: "app-staffing-resource-popup",
  templateUrl: "./staffing-resource-popup.component.html",
  styleUrls: ["./staffing-resource-popup.component.scss"]
})
export class StaffingResourcePopupComponent implements OnInit {
  employeeId: any;
  commitmentStartDate = new Date();
  today = new Date();

  weeklyArray: any = [];
  employeeDetails: any = {};
  employeeCommitments: any = [];
  commitmentTypes: CommitmentType[];

  hasInfoLoaded: boolean = false;
  hasCommitmentLoaded: boolean = false;

  constructor(private route: ActivatedRoute, private staffingService: StaffingResourcePopupService) {}

  ngOnInit(): void {
    this.employeeId = this.route.snapshot.queryParamMap.get("id");

    this.commitmentStartDate.setDate(
      this.commitmentStartDate.getDate() - ConstantsMaster.ganttConstants.initialDaysDeduction
    );

    this.getWeekStart();

    if (this.employeeId) {
      this.getEmployeeByCode(this.employeeId);
      this.getAllCommitmentsForEmployee(this.employeeId, this.commitmentStartDate);
    }
  }

  // create week and dates for commitments
  getWeekStart() {
    let dates = [];
    let monday = new Date();
    let day = new Date();

    monday = DateService.getStartOfWeek();
    day = DateService.getStartOfWeek();

    // set weeks
    for (var i = 0; i < 5; i++) {
      this.weeklyArray.push({ week: new Date(monday) });
      monday.setDate(monday.getDate() + 7);
    }

    // set dates
    for (var i = 0; i < 28; i++) {
      dates.push(new Date(day));
      day.setDate(day.getDate() + 1);
    }

    for (var i = 0; i < this.weeklyArray.length; i++) {
      this.weeklyArray[i]["dates"] = [];

      dates.forEach((date) => {
        if (
          new Date(date).getTime() >= new Date(this.weeklyArray[i].week).getTime() &&
          new Date(date).getTime() < new Date(this.weeklyArray[i + 1]?.week).getTime()
        ) {
          this.weeklyArray[i].dates.push(date);
        }
      });
    }

    this.weeklyArray = this.weeklyArray.slice(0, 4);
  }

  // get employee detail by id
  getEmployeeByCode(employeeCode) {
    this.staffingService.getEmployeeInfo(employeeCode).subscribe((details) => {
      // console.log("employee info", details);

      this.hasInfoLoaded = details ? true : false;
      this.employeeDetails = details;
    });

    const noteType = "RP";
    // this.staffingService.getResourceNotes(employeeCode, noteType).subscribe((note) => {
    //   console.log("note", note);
    // });
  }

  // get all employee commitments by id
  getAllCommitmentsForEmployee(employeeCode, effectiveDate) {
    let effectiveFromDate = effectiveDate
      ? DateService.convertDateInBainFormat(effectiveDate)
      : DateService.convertDateInBainFormat(this.today);
    effectiveFromDate = DateService.convertDateInBainFormat(effectiveFromDate);

    this.staffingService.getAllCommitmentsForEmployee(employeeCode, effectiveFromDate).subscribe((details) => {
      const commitments = this.getCommitments(details);
      // console.log("all commitments", commitments);

      this.hasCommitmentLoaded = commitments.length ? true : false;
      this.employeeCommitments = commitments;
    });
  }

  getCommitments(details) {
    const placeholderAllocatons = this.getPlaceholderCommitments(details.placeholderAllocations);
    const caseCommitments = this.getCaseOppCommitments(details.staffingAllocations);
    const otherCommitments = this.getOtherCommitments(details);

    let commitments = [];
    commitments = commitments.concat(placeholderAllocatons).concat(caseCommitments).concat(otherCommitments);

    return commitments;
  }

  getPlaceholderCommitments(placeholderAllocationDetails) {
    const placeholderParentId = CommonService.generate_UUID();
    //exclude confirmed placeholders on Case/Opps
    const placeholderAllocationsExceptConfirmed = placeholderAllocationDetails.filter(
      (x) => !((x.oldCaseCode || x.pipelineId) && x.isConfirmed)
    );
    let placeholderAllocations = placeholderAllocationsExceptConfirmed.map((a) => {
      return {
        parent: placeholderParentId,
        id: a.id,
        billingOfficeAbbreviation: a.billingOfficeAbbreviation,
        billingOfficeCode: a.billingOfficeCode,
        billingOfficeName: a.billingOfficeName,
        caseCode: a.caseCode,
        planningCardTitle: a.planningCardTitle,
        caseName: a.caseName,
        clientName: a.clientName,
        clientCode: a.clientCode,
        levelGrade: a.currentLevelGrade,
        oldCaseCode: a.oldCaseCode,
        pipelineId: a.pipelineId,
        caseTypeCode: a.caseTypeCode,
        opportunityName: a.opportunityName,
        startDate: a.startDate,
        endDate: a.endDate,
        commitmentTypeCode: CommitmentTypeEnum.NAMED_PLACEHOLDER,
        type: !a.planningCardId ? (!a.oldCaseCode ? "Opportunity" : "Case") : "PlanningCard",
        status: !a.oldCaseCode ? "" : "Active",
        description: !a.oldCaseCode
          ? !a.planningCardId
            ? a.probabilityPercent
              ? `${a.probabilityPercent}% - ${a.clientName} - ${a.opportunityName}`
              : `${a.clientName} - ${a.opportunityName}`
            : `${a.planningCardTitle}`
          : `${a.oldCaseCode} - ${a.clientName} - ${a.caseName}`,
        allocation: a.allocation,
        investmentCode: a.investmentCode,
        caseRoleCode: a.caseRoleCode,
        source: "staffing",
        caseStartDate: a.caseStartDate,
        caseEndDate: a.caseEndDate,
        opportunityStartDate: a.opportunityStartDate,
        opportunityEndDate: a.opportunityEndDate,
        notes: a.notes,
        caseRoleName: a.caseRoleName,
        planningCardId: a.planningCardId,
        isPlaceholderAllocation: a.isPlaceholderAllocation,
        isPlanningCardShared: a.isPlanningCardShared,
        isConfirmed: a.isConfirmed
      };
    });

    placeholderAllocations.sort((a, b) => {
      return <any>new Date(a.startDate) - <any>new Date(b.startDate);
    });

    if (placeholderAllocations.length > 0) {
      placeholderAllocations.push({
        id: placeholderParentId,
        startDate: placeholderAllocations[0].startDate,
        endDate: placeholderAllocations[placeholderAllocations.length - 1].endDate,
        commitmentTypeCode: "",
        type: "Placeholders"
      });
    }

    return placeholderAllocations;
  }

  getCaseOppCommitments(staffingAllocationDetails) {
    const caseOppCommitmentsParentId = CommonService.generate_UUID();
    let caseCommitments = staffingAllocationDetails.map((a) => {
      return {
        id: a.id,
        parent: caseOppCommitmentsParentId,
        caseCode: a.caseCode,
        caseName: a.caseName,
        clientName: a.clientName,
        clientCode: a.clientCode,
        levelGrade: a.currentLevelGrade,
        oldCaseCode: a.oldCaseCode,
        pipelineId: a.pipelineId,
        caseTypeCode: a.caseTypeCode,
        opportunityName: a.opportunityName,
        startDate: a.startDate,
        endDate: a.endDate,
        commitmentTypeCode: CommitmentTypeEnum.CASE_OPP,
        type: !a.oldCaseCode ? "Opportunity" : "Case",
        status: !a.oldCaseCode ? "" : "Active",
        description: !a.oldCaseCode
          ? a.probabilityPercent
            ? `${a.probabilityPercent}% - ${a.clientName} - ${a.opportunityName}`
            : `${a.clientName} - ${a.opportunityName}`
          : `${a.oldCaseCode} - ${a.clientName} - ${a.caseName}`,
        allocation: a.allocation,
        investmentCode: a.investmentCode,
        caseRoleCode: a.caseRoleCode,
        source: "staffing",
        caseStartDate: a.caseStartDate,
        caseEndDate: a.caseEndDate,
        opportunityStartDate: a.opportunityStartDate,
        opportunityEndDate: a.opportunityEndDate,
        notes: a.notes,
        caseRoleName: a.caseRoleName
      };
    });

    caseCommitments = caseCommitments
      ? caseCommitments?.filter((f) => <any>new Date(f.endDate) >= <any>this.commitmentStartDate.setHours(0, 0, 0, 0))
      : [];

    caseCommitments.sort((a, b) => {
      return <any>new Date(a.startDate) - <any>new Date(b.startDate);
    });

    if (caseCommitments.length > 0) {
      caseCommitments.push({
        id: caseOppCommitmentsParentId,
        startDate: caseCommitments[0].startDate,
        endDate: caseCommitments[caseCommitments.length - 1].endDate,
        commitmentTypeCode: "",
        type: "Confirmed"
      });
    }

    return caseCommitments;
  }

  getOtherCommitments(allDetails) {
    const otherCommitmentsId = CommonService.generate_UUID();
    // Trainings From BVU
    const trainings = allDetails.trainings.map((t) => {
      return {
        parent: otherCommitmentsId,
        startDate: t.startDate,
        endDate: t.endDate,
        commitmentTypeCode: CommitmentTypeEnum.TRAINING,
        type: t.type,
        description: `${t.role} - ${t.trainingName}`,
        allocation: "100%"
      };
    });

    // Commitments created in BOSS system except vacations and short term available
    // Vacations will be shown as split task in one row
    // Short term available will be checked for overlapping with LOA
    const commitmentsSavedInStaffing = allDetails.commitmentsSavedInStaffing
      ?.filter(
        (x) =>
          x.commitmentType.commitmentTypeCode !== CommitmentTypeCodeEnum.VACATION &&
          x.commitmentType.commitmentTypeCode !== CommitmentTypeCodeEnum.SHORT_TERM_AVAILABLE
      )
      .map((c) => {
        return {
          parent: otherCommitmentsId,
          id: c.id,
          startDate: c.startDate,
          endDate: c.endDate,
          type: c.commitmentType.commitmentTypeName,
          commitmentTypeCode: c.commitmentType.commitmentTypeCode,
          commitmentTypePrecedence: c.commitmentType.precedence,
          description: c.notes,
          allocation: c.allocation,
          source: c.isSourceStaffing ? "staffing" : "Other",
          employeeCode: c.employeeCode,
          notes: c.notes
        };
      });

    // Filter out short term availability if overlapped by LOA saved in workday
    let shortTermCommitmentSavedInStaffing = allDetails.commitmentsSavedInStaffing
      ?.filter((x) => x.commitmentType.commitmentTypeCode === CommitmentTypeCodeEnum.SHORT_TERM_AVAILABLE)
      .map((c) => {
        return {
          parent: otherCommitmentsId,
          id: c.id,
          startDate: c.startDate,
          endDate: c.endDate,
          type: c.commitmentType.commitmentTypeName,
          commitmentTypeCode: c.commitmentType.commitmentTypeCode,
          commitmentTypePrecedence: c.commitmentType.precedence,
          description: c.notes,
          source: c.isSourceStaffing ? "staffing" : "Other",
          employeeCode: c.employeeCode,
          notes: c.notes
        };
      });

    allDetails.loa.forEach((loa) => {
      shortTermCommitmentSavedInStaffing = shortTermCommitmentSavedInStaffing?.filter((st) => {
        const isOverlapped =
          moment(st.startDate).isSameOrAfter(moment(loa.startDate)) &&
          moment(st.endDate).isSameOrBefore(moment(loa.endDate));
        return !isOverlapped;
      });
    });

    // Vacation Requests from VRS
    const vacationId = CommonService.generate_UUID();
    const vacationSavedInVRS = allDetails.vacationRequests.map((v) => {
      return {
        parent: vacationId,
        startDate: v.startDate,
        endDate: v.endDate,
        commitmentTypeCode: CommitmentTypeEnum.VACATION,
        type: v.type,
        description: `${v.status} - ${v.description}`,
        allocation: "100%"
      };
    });

    const timeOffsSavedInWorkday = allDetails.employeeTimeOffs.map((v) => {
      return {
        parent: vacationId,
        startDate: v.startDate,
        endDate: v.endDate,
        commitmentTypeCode: CommitmentTypeEnum.VACATION,
        type: v.type,
        description: `${v.status}`,
        allocation: "100%"
      };
    });

    const vacationsSavedInStaffing = allDetails.commitmentsSavedInStaffing
      ?.filter((x) => x.commitmentType.commitmentTypeCode === CommitmentTypeCodeEnum.VACATION)
      .map((c) => {
        return {
          id: c.id,
          parent: vacationId,
          startDate: c.startDate,
          endDate: c.endDate,
          type: c.commitmentType.commitmentTypeName,
          commitmentTypeCode: c.commitmentType.commitmentTypeCode,
          commitmentTypePrecedence: c.commitmentType.precedence,
          description: c.notes,
          source: c.isSourceStaffing ? "staffing" : "Other",
          employeeCode: c.employeeCode,
          notes: c.notes
        };
      });

    let vacationRequests = vacationSavedInVRS
      .concat(vacationsSavedInStaffing)
      .sort((a, b) => <any>new Date(a.startDate) - <any>new Date(b.startDate));

    vacationRequests = vacationRequests
      .concat(timeOffsSavedInWorkday)
      .sort((a, b) => <any>new Date(a.startDate) - <any>new Date(b.startDate));

    if (vacationRequests.length > 0) {
      vacationRequests.push({
        parent: otherCommitmentsId,
        id: vacationId,
        startDate: vacationRequests[0].startDate,
        endDate: vacationRequests[vacationRequests.length - 1].endDate,
        commitmentTypeCode: CommitmentTypeEnum.VACATION,
        type: "Vacation",
        render: "split"
      });
    }

    // Office Holidays
    const holidayId = CommonService.generate_UUID();
    const officeHolidays = allDetails.officeHolidays
      .map((h) => {
        return {
          parent: holidayId,
          startDate: h.startDate,
          endDate: h.endDate,
          commitmentTypeCode: CommitmentTypeEnum.HOLIDAY,
          type: h.type,
          description: h.description,
          allocation: "100%"
        };
      })
      .sort((a, b) => <any>new Date(a.startDate) - <any>new Date(b.startDate));

    if (officeHolidays.length > 0) {
      officeHolidays.push({
        parent: otherCommitmentsId,
        id: holidayId,
        startDate: officeHolidays[0].startDate,
        endDate: officeHolidays[officeHolidays.length - 1].endDate,
        commitmentTypeCode: CommitmentTypeEnum.HOLIDAY,
        type: "Holiday",
        render: "split"
      });
    }

    // Transfers From Workday
    const transfers = allDetails.employeeTransfers
      .map((t) => {
        return {
          parent: otherCommitmentsId,
          startDate: t.startDate,
          endDate: t.startDate, // end Date is required by gantt
          commitmentTypeCode: CommitmentTypeEnum.TRANSFER,
          type: t.type,
          description: `Transfer from ${t.operatingOfficeCurrent.officeName} to ${t.operatingOfficeProposed.officeName}`
        };
      })
      .sort((a, b) => <any>new Date(a.startDate) - <any>new Date(b.startDate));

    const employeeTransition =
      allDetails.employeeTransition.startDate === null
        ? []
        : {
            // For Termination we are getting endDate only which is equivalent to termination effective date
            parent: otherCommitmentsId,
            startDate: allDetails.employeeTransition.startDate,
            endDate: allDetails.employeeTransition.endDate,
            commitmentTypeCode: CommitmentTypeEnum.TRANSITION,
            type: allDetails.employeeTransition.type
          };

    // Termination From Workday
    const employeeTermination =
      allDetails.employeeTermination.endDate === null
        ? []
        : {
            // For Termination we are getting endDate only which is equivalent to termination effective date
            parent: otherCommitmentsId,
            startDate: allDetails.employeeTermination.endDate,
            endDate: allDetails.employeeTermination.endDate,
            commitmentTypeCode: CommitmentTypeEnum.TERMINATION,
            type: allDetails.employeeTermination.type
          };

    //LOA from Workday
    const loas = allDetails.loa
      .map((t) => {
        return {
          parent: otherCommitmentsId,
          startDate: t.startDate,
          endDate: t.endDate,
          type: t.type,
          commitmentTypeCode: CommitmentTypeEnum.LOA,
          description: t.description
        };
      })
      .sort((a, b) => <any>new Date(a.startDate) - <any>new Date(b.startDate));

    let otherCommitments = [];
    otherCommitments = otherCommitments
      .concat(vacationRequests)
      .concat(officeHolidays)
      .concat(trainings)
      .concat(loas)
      .concat(employeeTransition)
      .concat(employeeTermination)
      .concat(commitmentsSavedInStaffing)
      .concat(shortTermCommitmentSavedInStaffing)
      .concat(transfers);

    // Sort commitments by start date Asc
    otherCommitments.sort((a, b) => {
      return <any>new Date(a.startDate) - <any>new Date(b.startDate);
    });

    if (otherCommitments.length > 0) {
      otherCommitments.push({
        id: otherCommitmentsId,
        startDate: otherCommitments[0].startDate,
        endDate: otherCommitments[otherCommitments.length - 1].endDate,
        commitmentTypeCode: "",
        type: "Commitments"
      });
    }

    return otherCommitments;
  }
}