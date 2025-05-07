// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';

// ----------------------- Component References ----------------------------------//
import { ProjectOverlayComponent } from '../project-overlay/project-overlay.component';
import { ResourceOverlayComponent } from '../resource-overlay/resource-overlay.component';

// ----------------------- Service References ----------------------------------//
import { CaseRollDialogService } from './caseRollDialog.service';
import { NotesDialogService } from './notesDialog.service';
import { OpportunityService } from '../behavioralSubjectService/opportunity.service';
import { QuickAddDialogService } from './quickAddDialog.service';
import { PlaceholderDialogService } from './placeholderDialog.service';
import { ResourceAssignmentService } from '../behavioralSubjectService/resourceAssignment.service';
import { ResourceCommitmentService } from '../behavioralSubjectService/resourceCommitment.service';
import { SkuCaseTermService } from '../behavioralSubjectService/skuCaseTerm.service';
import { SplitAllocationDialogService } from './splitAllocationDialog.service';
import { UserPreferenceService } from '../behavioralSubjectService/userPreference.service';

// --------------------------utilities -----------------------------------------//
import { MatDialogRef, MatDialog } from '@angular/material/dialog';
import { ResourceStaffableAsService } from '../behavioralSubjectService/resourceStaffableAs.service';
import { PlaceholderAssignmentService } from '../behavioralSubjectService/placeholderAssignment.service';

import { Store } from '@ngrx/store';
import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as ProjectAllocationsActions from 'src/app/state/actions/project-allocations.action';
import * as fromResourceCommitment from 'src/app/state/reducers/resource-commitment.reducer';
import * as ResourceCommitmentActions from 'src/app/state/actions/resource-commitment.action';
import * as ProjectOverlayActions from 'src/app/state/actions/project-overlay.action';
import * as ResourceOverlayActions from 'src/app/state/actions/resource-overlay.action';
import { PlaceholderAllocation } from 'src/app/shared/interfaces/placeholderAllocation.interface';
import { PlanningCardOverlayComponent } from '../planning-card-overlay/planning-card-overlay.component';
import { SharePlanningCardComponent } from 'src/app/shared/share-planning-card/share-planning-card.component';
import { SharePlanningCardDialogService } from './share-planning-card-dialog.service';
import * as PlanningCardOverlayActions from 'src/app/state/actions/planning-card-overlay.action';
import * as fromPlanningCardOverlayStore from 'src/app/state/reducers/planning-card-overlay.reducer';
import { staCommitmentDialogService } from './staCommitmentCaseOppDialog.service';

@Injectable()
export class OverlayDialogService {

  constructor(public dialog: MatDialog,
    private caseRollDialogService: CaseRollDialogService,
    private staCommitmentDialogService: staCommitmentDialogService,
    private notesDialogService: NotesDialogService,
    private quickAddDialogService: QuickAddDialogService,
    private placeholderDialogService: PlaceholderDialogService,
    private placeholderAssignmentService: PlaceholderAssignmentService,
    private resourceAssignmentService: ResourceAssignmentService,
    private resourceCommitmentService: ResourceCommitmentService,
    private skuCaseTermService: SkuCaseTermService,
    private splitAllocationDialogService: SplitAllocationDialogService,
    private userPreferenceService: UserPreferenceService,
    private opportunityService: OpportunityService,
    private resourceStaffableAsService: ResourceStaffableAsService,    
    private sharePlanningCardDialogService: SharePlanningCardDialogService,
    private projectAllocationsStore: Store<fromProjectAllocationsStore.State>,
    private planningCardStore: Store<fromPlanningCardOverlayStore.State>,
    private resourceCommitmentStore: Store<fromResourceCommitment.State>
    
  ) { }

  // --------------------------Local Variable -----------------------------------------//

  projectDialogRef: MatDialogRef<ProjectOverlayComponent, any>;
  dialogRef: MatDialogRef<ResourceOverlayComponent, any>;
  planningCardDialogRef: MatDialogRef<PlanningCardOverlayComponent, any>;
  // --------------------------Overlay -----------------------------------------//

  // Project Overlay
  openProjectDetailsDialogHandler(projectData) {

    // close previous project dialog & open new dialog
    if (this.projectDialogRef) {
      this.projectDialogRef.close('no null');
      this.projectDialogRef = null;
      
    }


    if(this.projectDialogRef!=null){
      this.projectDialogRef.beforeClosed().subscribe(result => {
        if (result !== 'no null') {
          this.projectDialogRef = null;
        }
      });
    }


    if (this.projectDialogRef == null) {
      this.projectDialogRef = this.dialog.open(ProjectOverlayComponent, {
        closeOnNavigation: true,
        hasBackdrop: false,
        enterAnimationDuration : 0,
        data: {
          projectData: projectData,
          showDialog: true
        }
      });
      
    }

    
    // Listens for click on resource name for opening the resource details pop-up
    this.projectDialogRef.componentInstance.openResourceDetailsFromProjectDialog.subscribe(employeeCode => {
      this.openResourceDetailsDialogHandler(employeeCode);
    });

    // Listens for click on notes opening the ag-grid notes pop-up
    this.projectDialogRef.componentInstance.openNotesDialog.subscribe(projectData => {
      this.notesDialogService.dialogRef = this.dialogRef;
      this.notesDialogService.projectDialogRef = this.projectDialogRef;
      this.notesDialogService.openNotesDialogHandler(projectData);
    });

    // Listens for click on split allocation in context menu of ag-grid
    this.projectDialogRef.componentInstance.openSplitAllocationDialog.subscribe(projectData => {
      this.splitAllocationDialogService.dialogRef = this.dialogRef;
      this.splitAllocationDialogService.projectDialogRef = this.projectDialogRef;
      this.splitAllocationDialogService.openSplitAllocationDialogHandler(projectData);
    });

    // inserts & updates resource data when changes are made to resource
    this.projectDialogRef.componentInstance.upsertResourceAllocationsToProject.subscribe(updatedData => {
      let upsertedAllocations:any = updatedData;
      this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.UpsertResourceAllocations({
          resourceAllocation: upsertedAllocations.resourceAllocation,
          splitSuccessMessage: upsertedAllocations.splitSuccessMessage,
          showMoreThanYearWarning: upsertedAllocations.showMoreThanYearWarning,
          allocationDataBeforeSplitting: upsertedAllocations.allocationDataBeforeSplitting,
          projectDialogRef: this.projectDialogRef, // null?
          resourceDialogRef: this.dialogRef
        })
        );
    });

    // inserts & updates placeholder data when changes are made to placeholder
    this.projectDialogRef.componentInstance.upsertPlaceholderAllocationsToProject.subscribe(updatedData => {
      let placeholderAllocations : PlaceholderAllocation[] = updatedData.placeholderAllocations;
        this.projectAllocationsStore.dispatch(
            new ProjectAllocationsActions.UpsertPlaceholderAllocations({
              placeholderAllocations: placeholderAllocations,
              projectDialogRef: this.projectDialogRef
            })
          )
    });

    // delete resources data
    this.projectDialogRef.componentInstance.deleteResourcesFromProject.subscribe(updatedData => {
      this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.DeleteResourceAllocationCommitment({
          allocation: [].concat(updatedData.resourceAllocation),
          allocationIds: updatedData?.allocationIds ?? "",
          commitmentIds: updatedData?.commitmentIds ?? "",
          notifyMessage: 'Assignment Deleted',
          projectDialogRef: this.projectDialogRef,
          resourceDialogRef: this.dialogRef
        })
      )
    });

    this.projectDialogRef.componentInstance.updateResourceCommitment.subscribe(updatedCommitment => {
      this.projectAllocationsStore.dispatch(
        new ResourceCommitmentActions.InsertResourceCommitment({
          commitments: [].concat(updatedCommitment.resourceAllocation),
          resourceDialogRef: this.dialogRef,
          projectDialogRef: this.projectDialogRef
        })
      )
    });

    // delete placeholder data
    this.projectDialogRef.componentInstance.deletePlaceholdersFromProject.subscribe(placeholderData => {
      this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.DeletePlaceholderAllocations({
            placeholderIds : placeholderData.placeholderIds,
            placeholderAllocation: placeholderData.placeholderAllocation,
            notifyMessage: placeholderData.notifyMessage, 
            projectDialogRef: this.projectDialogRef
        })
      )
    });

    // opens add resource popup
    this.projectDialogRef.componentInstance.openQuickAddForm.subscribe(projectData => {
      this.quickAddDialogService.dialogRef = this.dialogRef;
      this.quickAddDialogService.projectDialogRef = this.projectDialogRef;
      this.quickAddDialogService.openQuickAddFormHandler(projectData);
    });

    // insert sku case term
    this.projectDialogRef.componentInstance.insertSkuCaseTermsForProject.subscribe(skuTab => {
      this.projectAllocationsStore.dispatch(
        new ProjectOverlayActions.InsertSKUCaseTerms({
            skuTerms: skuTab.skuTerms,
            skuTab : skuTab.skuTab, 
            projectDialogRef: this.projectDialogRef
        })
      )
    });

    // update sku case term
    this.projectDialogRef.componentInstance.updateSkuCaseTermsForProject.subscribe(skuTab => {
      this.projectAllocationsStore.dispatch(
        new ProjectOverlayActions.UpdateSKUCaseTerms({
            skuTerms: skuTab.skuTerms,
            skuTab : skuTab.skuTab, 
            projectDialogRef: this.projectDialogRef
        })
      )
    });

    // delete sku case term
    this.projectDialogRef.componentInstance.deleteSkuCaseTermsForProject.subscribe(skuTab => {
      this.projectAllocationsStore.dispatch(
        new ProjectOverlayActions.DeleteSKUCaseTerms({
            skuTab : skuTab, 
            projectDialogRef: this.projectDialogRef
        })
      )
    });

    // add project to user settings show list
    this.projectDialogRef.componentInstance.addProjectToUserExceptionShowList.subscribe(event => {
      this.userPreferenceService.addCaseOpportunityToUserExceptionShowList(event);
    });

    // remove project to user settings show list
    this.projectDialogRef.componentInstance.removeProjectFromUserExceptionShowList.subscribe(event => {
      this.userPreferenceService.removeCaseOpportunityFromUserExceptionShowList(event);
    });

    // add project to user settings hide list
    this.projectDialogRef.componentInstance.addProjectToUserExceptionHideList.subscribe(event => {
      this.userPreferenceService.addCaseOpportunityToUserExceptionHideList(event);
    });

    // remove project from user settings hide list
    this.projectDialogRef.componentInstance.removeProjectFromUserExceptionHideList.subscribe(event => {
      this.userPreferenceService.removeCaseOpportunityFromUserExceptionHideList(event, true);
    });

    // open case roll pop-up
    this.projectDialogRef.componentInstance.openCaseRollForm.subscribe(event => {
      this.caseRollDialogService.projectDialogRef = this.projectDialogRef;
      this.caseRollDialogService.openCaseRollFormHandler(event);
    });

    this.projectDialogRef.componentInstance.openStaCommitmentForm.subscribe(event => {
      this.staCommitmentDialogService.projectDialogRef = this.projectDialogRef;
      this.staCommitmentDialogService.openSTACommitmentFormHandler(event);
    });





  
     // open placeholder pop-up
     this.projectDialogRef.componentInstance.openPlaceholderForm.subscribe(projectData => {
      this.placeholderDialogService.projectDialogRef = this.projectDialogRef;
      this.placeholderDialogService.openPlaceholderFormHandler(projectData);
    });

    // update pipeline data in staffing db
    this.projectDialogRef.componentInstance.updateProjectChanges.subscribe(event => {
      if(event.pipelineId) {
      this.projectAllocationsStore.dispatch(
        new ProjectOverlayActions.UpdateOpportunity({
            event : event, 
            projectDialogRef: this.projectDialogRef
        })
      )} else if(event.oldCaseCode) {
        this.projectAllocationsStore.dispatch(
          new ProjectOverlayActions.UpdateCase({
              event : event, 
              projectDialogRef: this.projectDialogRef
          })
        )
      }
    });

    
    // }
  }

  openResourceDetailsDialogHandler(employeeCode) {
    // if (this.createTeamDialogRef != null) {
    //   this.createTeamDialogRef.close();
    // }

    // close previous resource dialog & open new dialog
    if (this.dialogRef) {
      this.dialogRef.close('no null');
      this.dialogRef = null;
    }

    if(this.dialogRef!=null)
    {
      this.dialogRef.beforeClosed().subscribe((result) => {
        if (result !== 'no null') {
          this.dialogRef = null;
        }
      });
    }
    
    if(this.dialogRef == null)
    {
      this.dialogRef = this.dialog.open(ResourceOverlayComponent, {
        closeOnNavigation: true,
        hasBackdrop: false,
        enterAnimationDuration : 0,
        data: {
          employeeCode: employeeCode,
          showOverlay: true
        }
      });
    }

    this.dialogRef.componentInstance.openResourceDetailsFromProjectDialog.subscribe(projectData => {
      this.openResourceDetailsDialogHandler(projectData);
    });

    // Listens for click on case name for opening the project details pop-up
    this.dialogRef.componentInstance.openProjectDetailsFromResourceDialog.subscribe(projectData => {
      this.openProjectDetailsDialogHandler(projectData);
    });

    // Listens for click on notes opening the ag-grid notes pop-up
    this.dialogRef.componentInstance.openNotesDialog.subscribe(projectData => {
      this.notesDialogService.dialogRef = this.dialogRef;
      this.notesDialogService.projectDialogRef = this.projectDialogRef;
      this.notesDialogService.openNotesDialogHandler(projectData);
    });

    // Listens for click on split allocation in context menu of ag-grid
    this.dialogRef.componentInstance.openSplitAllocationDialog.subscribe(projectData => {
      this.splitAllocationDialogService.dialogRef = this.dialogRef;
      this.splitAllocationDialogService.projectDialogRef = this.projectDialogRef;
      this.splitAllocationDialogService.openSplitAllocationDialogHandler(projectData);
    });

    this.dialogRef.componentInstance.updateResourceCommitment.subscribe(updatedCommitment => {
      this.resourceCommitmentStore.dispatch(
        new ResourceCommitmentActions.InsertResourceCommitment({
          commitments: [].concat(updatedCommitment.resourceAllocation),
          resourceDialogRef: this.dialogRef,
          projectDialogRef: this.projectDialogRef
        })
      )
    });


    // inserts & updates resource data when changes are made to resource
    this.dialogRef.componentInstance.upsertResourceAllocationsToProject.subscribe(updatedData => {
      let upsertedAllocations : any = updatedData;
      this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.UpsertResourceAllocations({
          resourceAllocation: upsertedAllocations.resourceAllocation,
          splitSuccessMessage: upsertedAllocations.splitSuccessMessage,
          showMoreThanYearWarning: upsertedAllocations.showMoreThanYearWarning,
          allocationDataBeforeSplitting: upsertedAllocations.allocationDataBeforeSplitting,
          projectDialogRef: this.projectDialogRef,
          resourceDialogRef: this.dialogRef
        })
        );
    });

    this.dialogRef.componentInstance.deleteResourceCommitment.subscribe(deletedObj => {
      this.resourceCommitmentService.deleteResourceCommitment(deletedObj, this.dialogRef);   
    });

    this.dialogRef.componentInstance.deleteResourceAllocationFromCase.subscribe(allocation => {
      this.resourceAssignmentService.deleteResourceAssignmentFromCase(allocation.allocationId, this.dialogRef, this.projectDialogRef);
      this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(allocation.resourceAllocation);
    });

    this.dialogRef.componentInstance.deleteResourceAllocationFromCases.subscribe(allocation => {
      this.resourceAssignmentService.deleteResourcesAssignmentsFromCase(allocation.allocationIds, this.dialogRef, this.projectDialogRef);
      this.resourceAssignmentService.deletePlaygroundAllocationsForCasePlanningMetrics(allocation.resourceAllocation);
    });

    this.dialogRef.componentInstance.deleteResourceAllocationsCommitmentsFromCase.subscribe(dataToDelete => {
      this.projectAllocationsStore.dispatch(
        new ProjectAllocationsActions.DeleteResourceAllocationCommitment({
          allocation: [].concat(dataToDelete.resourceAllocation),
          allocationIds: dataToDelete.allocationIds,
          commitmentIds: dataToDelete.commitmentIds,
          notifyMessage: 'Assignment Deleted',
          projectDialogRef: this.projectDialogRef,
          resourceDialogRef: this.dialogRef
        })
      )
    });

    this.dialogRef.componentInstance.deleteStaffableAsRoleEmitter.subscribe(event => {
      this.resourceCommitmentStore.dispatch(
        new ResourceOverlayActions.DeleteResourceStaffableAs({
          staffableRoleToBeDeleted : event, 
          resourceDialogRef: this.dialogRef
      }))
    });

    this.dialogRef.componentInstance.upsertStaffableAsRoleEmitter.subscribe(event => {
      let resource = event.resource;
      event.staffableRoles.map(x => {
          x.employeeCode = resource.employeeCode,
              x.levelGrade = resource.levelGrade
      });
      this.resourceCommitmentStore.dispatch(
        new ResourceOverlayActions.UpsertResourceStaffableAs({
          staffableRoles : event.staffableRoles, 
          resourceDialogRef: this.dialogRef
      })
      )
    });

    this.dialogRef.componentInstance.openQuickAddForm.subscribe(modalData => {
      this.quickAddDialogService.dialogRef = this.dialogRef;
      this.quickAddDialogService.projectDialogRef = this.projectDialogRef;
      this.quickAddDialogService.openQuickAddFormHandler(modalData);
    });

    // updates resource data when changes are made to resource
    this.dialogRef.componentInstance.updateResourceDataForProject.subscribe(updatedData => {
      this.resourceAssignmentService.updateResourceAssignmentToCase(updatedData, this.dialogRef, this.projectDialogRef);
    });

  }


// Planning Card Overlay
openPlanningCardDetailsDialogHandler(planningCardId) {
  // Close previously opened dialog and open new dialog
  if (this.planningCardDialogRef) {
      this.planningCardDialogRef.close("no null");
      this.dialogRef = null;
  }

  this.planningCardDialogRef = this.dialog.open(PlanningCardOverlayComponent, {
      closeOnNavigation: true,
      hasBackdrop: false,
      enterAnimationDuration : 0,
      data: {
          showDialog: true,
          planningCardId: planningCardId
      }
  });

  this.planningCardDialogRef.beforeClosed().subscribe((result) => {
      if (result !== "no null") {
          this.planningCardDialogRef = null;
      }
  });

   // inserts & updates placeholder data when changes are made to placeholder
   this.planningCardDialogRef.componentInstance.upsertPlaceholderAllocationsToProject.subscribe(updatedData => {
    let placeholderAllocations : PlaceholderAllocation[] = updatedData.placeholderAllocations;
      this.projectAllocationsStore.dispatch(
          new ProjectAllocationsActions.UpsertPlaceholderAllocations({
            placeholderAllocations: placeholderAllocations,
            planningCardDialogRef: this.planningCardDialogRef
          })
        )
  });
  // delete placeholder data
  this.planningCardDialogRef.componentInstance.deletePlaceholdersFromProject.subscribe(placeholderData => {
    this.projectAllocationsStore.dispatch(
      new ProjectAllocationsActions.DeletePlaceholderAllocations({
          placeholderIds : placeholderData.placeholderIds,
          placeholderAllocation: placeholderData.placeholderAllocation,
          notifyMessage: 'Assignment Deleted', 
          planningCardDialogRef: this.planningCardDialogRef
      })
    )
  });

    // Listens for click on resource name for opening the resource details pop-up
  this.planningCardDialogRef.componentInstance.openResourceDetailsFromProjectDialog.subscribe(employeeCode => {
      this.openResourceDetailsDialogHandler(employeeCode);
  });

   // open placeholder pop-up
   this.planningCardDialogRef.componentInstance.openPlaceholderForm.subscribe(projectData => {
    this.placeholderDialogService.planningCardDialogRef = this.planningCardDialogRef;
    this.placeholderDialogService.openPlaceholderFormHandler(projectData);
  });

  this.planningCardDialogRef.componentInstance.openSTACommitmentForm.subscribe(event => {
    this.staCommitmentDialogService.planningCardDialogRef = this.planningCardDialogRef;
    this.staCommitmentDialogService.openSTACommitmentFormHandler(event);
  });

   // opens add resource popup
   this.planningCardDialogRef.componentInstance.openQuickAddForm.subscribe(projectData => {
    this.quickAddDialogService.dialogRef = this.dialogRef;
    this.quickAddDialogService.planningCardDialogRef = this.planningCardDialogRef;
    this.quickAddDialogService.openQuickAddFormHandler(projectData);
  });

  this.planningCardDialogRef.componentInstance.openSharePlanningCardDialog.subscribe(planningCardData => {
    this.sharePlanningCardDialogService.planningCardDialogRef = this.planningCardDialogRef;
    this.sharePlanningCardDialogService.openSharePlanningCardDialogHandler(planningCardData);
  });

  this.planningCardDialogRef.componentInstance.openSplitAllocationDialog.subscribe(projectData => {
    this.splitAllocationDialogService.dialogRef = this.dialogRef;
    this.splitAllocationDialogService.planningCardDialogRef = this.planningCardDialogRef;
    this.splitAllocationDialogService.openSplitAllocationDialogHandler(projectData);
  });

  this.planningCardDialogRef.componentInstance.updatePlanningCard.subscribe((event) => {
    this.planningCardStore.dispatch(
      new PlanningCardOverlayActions.UpsertPlanningCard({
        planningCard: event.planningCard,
        planningCardDialogRef: this.planningCardDialogRef 
    })
    )
  });

}

  closeDialogs() {

    this.dialogRef?.close();
    this.projectDialogRef?.close();
    this.planningCardDialogRef?.close();

  }

}
