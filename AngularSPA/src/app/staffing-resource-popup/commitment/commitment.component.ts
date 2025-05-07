import { Component, OnInit, Input } from "@angular/core";
import * as moment from "moment";
import { CommitmentType as CommitmentTypeCodeEnum } from "src/app/shared/constants/enumMaster";
import { StaffingCommitment } from "src/app/shared/interfaces/staffingCommitment.interface";

// services
import { CoreService } from "src/app/core/core.service";
import { CommonService } from "src/app/shared/commonService";

@Component({
  selector: "app-commitment",
  templateUrl: "./commitment.component.html",
  styleUrls: ["./commitment.component.scss"]
})
export class CommitmentComponent implements OnInit {
  @Input() commitment: StaffingCommitment;
  @Input() dateRange: [Date, Date];

  offsetLeft;
  commitmentDurationInDays: any;

  isRowCollapsed: boolean = false;
  isTopbarCollapsed: boolean = false;

  className: string = "";
  commitmentDescription: string = "";

  gridSize: number = 32;
  gridCellMargin: number = 6;

  constructor(private coreService: CoreService) {}

  ngOnInit(): void {
    this.setCommitmentClasses();
    this.getCommitmentDescriptionAndToolTipText();
  }

  setCommitmentClasses(reAdjustClass = false, $event = null) {
    let startCount = 0;

    const commitmentStartDate = moment(this.commitment.startDate).startOf("day");
    const dateRangeStartDate = moment(this.dateRange[0]).startOf("day");
    const commitmentEndDate = moment(this.commitment.endDate).startOf("day");
    const dateRangeEndDate = moment(this.dateRange[1]).startOf("day");

    this.commitmentDurationInDays = commitmentEndDate.diff(commitmentStartDate, "days");

    if (commitmentStartDate.isAfter(dateRangeStartDate)) {
      startCount = commitmentStartDate.diff(dateRangeStartDate, "days") + 1;
    }

    const end = commitmentEndDate.isAfter(dateRangeEndDate) ? dateRangeEndDate : commitmentEndDate;
    const start = commitmentStartDate.isAfter(dateRangeStartDate) ? commitmentStartDate : dateRangeStartDate;
    const duration = end.diff(start, "days") + 1;

    this.commitment["duration"] = duration;

    if (reAdjustClass) {
      this.offsetLeft = (startCount - 1) * this.gridSize + this.gridCellMargin;
      $event.host.classList.add(`duration-${duration}`);
    }

    let border = "";

    if (this.commitment.commitmentTypeCode === CommitmentTypeCodeEnum.PLANNING_CARD) {
      border = "border-dotted";
    }

    this.className =
      "start-" + startCount + " duration-" + duration + " commitment-" + this.getCommitmentColorClass() + " " + border;
  }

  getCommitmentColorClass(): string {
    let colorClass = "";

    if (this.isTopbarCollapsed == false || this.isRowCollapsed == false) {
      colorClass = this.getCommitmentColorClassExpanded();
    } else if (this.isTopbarCollapsed || this.isRowCollapsed) {
      colorClass = this.getCommitmentColorClassCollapsed();
    } else {
      colorClass = this.getCommitmentColorClassCollapsed();
    }

    return colorClass;
  }

  getCommitmentColorClassCollapsed(): string {
    let colorClass = "";

    switch (this.commitment.commitmentTypeCode) {
      case CommitmentTypeCodeEnum.CASE_OPP:
      case CommitmentTypeCodeEnum.PLANNING_CARD:
      case CommitmentTypeCodeEnum.NAMED_PLACEHOLDER: {
        colorClass = "blue";
        break;
      }
      case CommitmentTypeCodeEnum.SHORT_TERM_AVAILABLE:
      case CommitmentTypeCodeEnum.NOT_AVAILABLE:
      case CommitmentTypeCodeEnum.LIMITED_AVAILABILITY:
      case CommitmentTypeCodeEnum.AAG:
      case CommitmentTypeCodeEnum.ADAPT:
      case CommitmentTypeCodeEnum.FRWD:
      case CommitmentTypeCodeEnum.VACATION:
      case CommitmentTypeCodeEnum.LOA:
      case CommitmentTypeCodeEnum.TRAINING:
      case CommitmentTypeCodeEnum.RECRUITING:
      case CommitmentTypeCodeEnum.HOLIDAY:
      case CommitmentTypeCodeEnum.PEG:
      case CommitmentTypeCodeEnum.PEG_Surge:
      case CommitmentTypeCodeEnum.DOWN_DAY: {
        colorClass = "purple";
        break;
      }
      default: {
        colorClass = "purple";
        break;
      }
    }

    return colorClass;
  }

  getCommitmentColorClassExpanded(): string {
    return CommonService.getCommitmentColorClass(this.commitment.commitmentTypeCode, this.commitment.investmentCode,this.commitment.pipelineId,this.commitment.oldCaseCode, this.commitment.includeInCapacityReporting, this.commitment.status ?? null);
  }

  isCommitmentAccessible(commitment) {
    let editableCommitments: string[] = CommonService.getEditableCommitmentTypesCodesList(
      this.coreService.loggedInUserClaims
    );

    return editableCommitments.some((x) => x === commitment.commitmentTypeCode);
  }

  // commitment description & tooltip
  getCommitmentDescriptionAndToolTipText() {
    if (
      this.commitment.commitmentTypeCode == CommitmentTypeCodeEnum.CASE_OPP ||
      this.commitment.commitmentTypeCode === CommitmentTypeCodeEnum.NAMED_PLACEHOLDER ||
      this.commitment.commitmentTypeCode === CommitmentTypeCodeEnum.PLANNING_CARD
    ) {
      this.getCommitmentDescriptionAndToolTipTextForAllocations();
    } else {
      this.getCommitmentDescriptionAndToolTipTextForAllOtherCommitments();
    }
  }

  getCommitmentDescriptionAndToolTipTextForAllocations() {
    const investmentName = this.commitment.investmentName ? " - " + this.commitment.investmentName : "";
    const caseRoleName = this.commitment.caseRoleName ? " - " + this.commitment.caseRoleName : "";

    const formattedStartDate = moment(this.commitment.startDate).format("DD MMM YYYY");
    const formattedEndDate = moment(this.commitment.endDate).format("DD MMM YYYY");
    
    const namedPlaceholderText =
      this.commitment.commitmentTypeCode === CommitmentTypeCodeEnum.NAMED_PLACEHOLDER ? "PLACEHOLDER - " : "";

    switch (this.commitment.commitmentTypeCode) {
      case CommitmentTypeCodeEnum.CASE_OPP:
      case CommitmentTypeCodeEnum.NAMED_PLACEHOLDER: {
        if (this.commitment.oldCaseCode) {
          this.commitmentDescription = `${namedPlaceholderText}Case - ${this.commitment.oldCaseCode} - ${
            this.commitment.caseName + investmentName + caseRoleName
          } (${this.commitment.allocation}%) (${formattedStartDate} - ${formattedEndDate})`;
        } else if (this.commitment.pipelineId) {
          this.commitmentDescription = `${namedPlaceholderText}Opportunity - ${
            this.commitment.opportunityName + investmentName + caseRoleName
          } (${this.commitment.allocation}%) (${formattedStartDate} - ${formattedEndDate})`;
        }

        break;
      }
      case CommitmentTypeCodeEnum.PLANNING_CARD: {
        this.commitmentDescription = `Planning Card - ${this.commitment.planningCardName} (${this.commitment.allocation}%) (${formattedStartDate} - ${formattedEndDate})`;

        break;
      }
    }
  }

  getCommitmentDescriptionAndToolTipTextForAllOtherCommitments() {
    const description =
      this.commitment.description && this.commitment.description != ""
        ? this.commitment.type + " - " + this.commitment.description
        : this.commitment.type;
    this.commitmentDescription = description;
  }
}
