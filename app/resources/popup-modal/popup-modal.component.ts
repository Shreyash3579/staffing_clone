import { Component, OnInit } from "@angular/core";
import { BsModalRef } from "ngx-bootstrap/modal";
import { CommonService } from "src/app/shared/commonService";
import { CommitmentType as CommitmentTypeCodeEnum } from "src/app/shared/constants/enumMaster";
import { StaffingCommitment } from "src/app/shared/interfaces/staffingCommitment.interface";

@Component({
  selector: "app-popup-modal",
  templateUrl: "./popup-modal.component.html",
  styleUrls: ["./popup-modal.component.scss"]
})
export class PopupModalComponent implements OnInit {
  public commitments: StaffingCommitment[];
  commitmentsData = [];

  // ----------------------- Life Cycle Events ----------------------------------//
  constructor(public modalRef: BsModalRef) { }

  ngOnInit(): void {
    this.convertToFormattedCommitments();
  }

  // ----------------------- Private methods ----------------------------------//

  convertToFormattedCommitments(){
    const formattedCommitments = [];
    this.commitments.forEach(commitment => {
      const headerText = this.getCommitmentHeaderText(commitment);
      const commitmentColor = this.getCommitmentColorClass(commitment);

      formattedCommitments.push({
        title : headerText,
        commitmentColor: commitmentColor,
        commitmentTypeCode : commitment.commitmentTypeCode,
        commitmentTypeName: commitment.commitmentTypeName,
        employeeName: commitment.employeeName,
        projectName: commitment.caseName || commitment.opportunityName || commitment.planningCardName,
        clientName: commitment.clientName,
        startDate: commitment.caseStartDate || commitment.opportunityStartDate || commitment.startDate,
        endDate: commitment.caseEndDate || commitment.opportunityEndDate || commitment.endDate,
        allocation: commitment.allocation,
        investmentCode : commitment.investmentCode,
        pipelineId : commitment.pipelineId,
        oldCaseCode :commitment.oldCaseCode,
        includeInCapacityReporting: commitment.includeInCapacityReporting,
        status: commitment.status??null,
      });
    });
    
    this.sortAllocationsAndCommitments(formattedCommitments);
  }

  getCommitmentHeaderText(commitment: StaffingCommitment){
    let headerText =  "";
    switch(commitment.commitmentTypeCode){
      case CommitmentTypeCodeEnum.CASE_OPP:{
        if(commitment.oldCaseCode)
          headerText = `Case - ${commitment.oldCaseCode}`;
        else
          headerText = `Opportunity - ${commitment.probabilityPercent}`;
        break;
      }

      case CommitmentTypeCodeEnum.NAMED_PLACEHOLDER:{
        if(commitment.oldCaseCode)
          headerText = `Placeholder - Case - ${commitment.oldCaseCode}`;
        else
          headerText = `Placeholder - Opportunity - ${commitment.probabilityPercent}`;
        break;
      }
      default: {
        headerText = commitment.commitmentTypeName;
        break;
      }
    }

    return headerText;
  }

  sortAllocationsAndCommitments(formattedCommitments) {
    let allocations = formattedCommitments.filter(x =>
      x.commitmentTypeCode === CommitmentTypeCodeEnum.CASE_OPP
      || x.commitmentTypeCode === CommitmentTypeCodeEnum.PLANNING_CARD)

    allocations = allocations.sort((a, b) => {
      return this.sortAllocations(a, b);
    });

    
    let commitments = formattedCommitments.filter(f => !allocations.includes(f));

    commitments = commitments.sort((a, b) => {
      return this.sortCommitments(a, b);
    });

    this.commitmentsData = this.commitmentsData.concat(allocations).concat(commitments);
  }

  getCommitmentColorClass(commitment) : string{
    return CommonService.getCommitmentColorClass(commitment.commitmentTypeCode, commitment.investmentCode,commitment.pipelineId,commitment.oldCaseCode,commitment.includeInCapacityReporting,commitment.status??null);
  }

  private sortAllocations(previousElement, nextElement) {
    let sortValue = CommonService.sortByDate(previousElement.endDate, nextElement.endDate);
    if (sortValue === 0) {
      sortValue = CommonService.sortByString(previousElement.clientName, nextElement.clientName);
    }
    return sortValue;
  }

  private sortCommitments(previousElement, nextElement,) {
    let sortValue = CommonService.sortByDate(previousElement.endDate, nextElement.endDate);
    if (sortValue === 0) {
      sortValue = CommonService.sortByString(previousElement.commitmentType, nextElement.commitmentType);
    }
    return sortValue;
  }
}
