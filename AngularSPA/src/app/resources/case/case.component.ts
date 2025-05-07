import { Component, OnInit, Input, Output, EventEmitter, SimpleChanges } from "@angular/core";
import { Project } from "src/app/shared/interfaces/project.interface";
import { ProjectBasic } from "src/app/shared/interfaces/project.interface";
import { ResourcesCount } from "src/app/shared/interfaces/resourcesCount.interface";

@Component({
  selector: "resources-case",
  templateUrl: "./case.component.html",
  styleUrls: ["./case.component.scss"]
})
export class CaseComponent implements OnInit {
   // -----------------------Input Events-----------------------------------------------//
  @Input() case: ProjectBasic;
  @Input() rowIndex = "";
  @Input() isCaseGroupCollapsed: boolean;
  @Input() projectmembers: Project[];
  @Input() project: Project;
  @Input() resourcesCountOnCaseOpp: ResourcesCount[];
  visible: boolean;
  
  
  // -----------------------Output Events-----------------------------------------------//
  @Output() collapseExpandCaseGroupEmitter = new EventEmitter<boolean>();
  @Output() openOverlappedTeamsPopup = new EventEmitter<any>();

  // -----------------------Life Cycle Events-----------------------------------------------//
  constructor(
  ) { }

  
  ngOnInit() {
    this.isPersistentTeamPopUpVisible(this.case);
  }

  ngOnChanges(changes : SimpleChanges){
    //console.log(changes['resourcesCountOnCaseOpp'].currentValue);
  }

  // -----------------------Event Handlers-----------------------------------------------//
  toggleExpandCollapseCaseGroup(){
    this.isCaseGroupCollapsed = !this.isCaseGroupCollapsed;
    this.collapseExpandCaseGroupEmitter.emit(this.isCaseGroupCollapsed);
  }

  openPersistentTeamPopupHandler(){
    const modalData = {
      projectData: this.case,
      overlappedTeams : null,
      allocation: this.getAllocation(this.case)
    };

    this.openOverlappedTeamsPopup.emit(modalData);
  }

  isPersistentTeamPopUpVisible(caseData:ProjectBasic): boolean {
    const visible = this.getCountOfResources(caseData)
    return visible;
  }

  getAllocation(caseData : ProjectBasic){

    if(caseData?.oldCaseCode)
      return this.projectmembers[0].allocations.find(x=>x.oldCaseCode==caseData?.oldCaseCode)
    else
      return this.projectmembers[0].allocations.find(x=>x.pipelineId==caseData?.pipelineId)
  }

  getCountOfResources(caseData : ProjectBasic): boolean{
    let value;

    if(caseData?.oldCaseCode)
      value = this.resourcesCountOnCaseOpp?.find(x=>x.oldCaseCode==caseData?.oldCaseCode)?.resourceCount === 1 ? true : false
    else
      value = this.resourcesCountOnCaseOpp?.find(x=>x.pipelineId==caseData?.pipelineId)?.resourceCount === 1 ? true : false
      
    return value
  }
  // private expandRow(rowElement: HTMLElement = null, resourceElement: HTMLElement = null) {
  //   // Row Element "tr"
  //   if (!rowElement) {
  //     rowElement = document.querySelector<HTMLElement>(
  //       `#gantt-row-${this.rowIndex}`
  //     );
  //   }

  //   // Element that was clicked
  //   if (!resourceElement) {
  //     resourceElement = document.querySelector<HTMLElement>(
  //       `#btn-expand-collapse-${this.rowIndex}`
  //     );
  //   }

  //   // People Wrapper
  //   const peopleWrapperElement = document.querySelector<HTMLElement>(
  //     `#gantt-row-${this.rowIndex} .people-wrapper`
  //   );

  //   // Utilization Rows
  //   const utilizationRowsElement = document.querySelector<HTMLElement>(
  //     `#utilization-index-${this.rowIndex}`
  //   );

  //   let elements: HTMLElement[] = []
  //   elements.push(rowElement, resourceElement, utilizationRowsElement, peopleWrapperElement);

  //   CommonService.toggleClass(elements, 'collapsed');
  // }

}
