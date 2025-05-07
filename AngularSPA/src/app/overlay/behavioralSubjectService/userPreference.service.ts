// ----------------------- Angular Package References ----------------------------------//
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

// ----------------------- Service References ----------------------------------//
import { CoreService } from 'src/app/core/core.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { OverlayMessageService } from './overlayMessage.service';
import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { UserPreferenceSupplyGroup } from 'src/app/shared/interfaces/userPreferenceSupplyGroup';
import { UserPreferencesService } from 'src/app/core/user-preferences.service';
import { UserPreferencesMessageService } from 'src/app/core/user-preferences-message.service';
import { Store } from '@ngrx/store';
import * as fromProjectAllocationsStore from 'src/app/state/reducers/project-allocations.reducer';
import * as ProjectOverlayActions from 'src/app/state/actions/project-overlay.action';

@Injectable({ providedIn: 'root' })
export class UserPreferenceService {
  // -----------------------Local Variables--------------------------------------------//
  private destroy$: Subject<boolean> = new Subject<boolean>();
  // -----------------------Constructor--------------------------------------------//
  constructor(
    private coreService: CoreService,
    private userPreferencesService: UserPreferencesService,
    private overlayMessageService: OverlayMessageService,
    private notifyService: NotificationService,
    private userPreferencesMessageService: UserPreferencesMessageService,
    private projectAllocationsStore: Store<fromProjectAllocationsStore.State>) { }

  // -----------------------Helper Function--------------------------------------------//

  updateDemandSortOrder(event) {
    const userPreferences = this.coreService.getUserPreferencesValue();
    if (event?.planningCardsSortOrder) {
      userPreferences.planningCardsSortOrder = event.planningCardsSortOrder;
    }
    if (event?.caseOppSortOrder) {
      userPreferences.caseOppSortOrder = event.caseOppSortOrder;
    }
    this.overlayMessageService.updateUserPreference(userPreferences);
    this.coreService.updateUserPreferences(userPreferences)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        (data) => {
          this.coreService.setUserPreferences(data, false);
          this.notifyService.showSuccess('Custom sort order updated');
        },
        (error) => {
          this.notifyService.showError('Error while removing');
        }
      );
  }

  updatePinForHistoricalDemands(isPinned){
    const userPreferences = this.coreService.getUserPreferencesValue();
    userPreferences.isHistoricalDemandPinned = isPinned;

    this.overlayMessageService.updateUserPreference(userPreferences);
    this.coreService.updateUserPreferences(userPreferences)
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        (data) => {
          this.coreService.setUserPreferences(data, false);
          this.notifyService.showSuccess('Historical Demands Pin/Unpin updated');
        },
        (error) => {
          this.notifyService.showError('Error while updating');
        }
      );
  }

  addCaseOpportunityToUserExceptionShowList({ oldCaseCode, pipelineId }) {
    const userPreferences = this.removeCaseOpportunityFromUserExceptionHideList({ oldCaseCode, pipelineId }, false);
    if (oldCaseCode && userPreferences) {
      const caseExceptionShowList = userPreferences.caseExceptionShowList ? userPreferences.caseExceptionShowList.split(',') : [];
      if (!caseExceptionShowList.includes(oldCaseCode)) {
        caseExceptionShowList.push(oldCaseCode);
        userPreferences.caseExceptionShowList = caseExceptionShowList.toString();
      }
    } else if (pipelineId && userPreferences) {
      const opportunityExceptionShowList = userPreferences.opportunityExceptionShowList ?
        userPreferences.opportunityExceptionShowList.split(',') : [];
      if (!opportunityExceptionShowList.includes(pipelineId)) {
        opportunityExceptionShowList.push(pipelineId);
        userPreferences.opportunityExceptionShowList = opportunityExceptionShowList.toString();
      }
    }

    userPreferences.caseOppSortOrder = this.addCaseOppToUserPreferenceSortOrder(oldCaseCode ?? pipelineId);
    this.overlayMessageService.updateUserPreference(userPreferences);

    this.projectAllocationsStore.dispatch(
      new ProjectOverlayActions.UpdateUserPreferencesForPinOrHide({ 
        oldCaseCode,
        pipelineId,
        notifyMessage: 'pinned successfully',
        userPreferences,
        action: 'pin' 
      })
    )
    //this.upsertUserPreferences(oldCaseCode, pipelineId, 'pinned successfully', userPreferences);

    return userPreferences;
  }

  removeCaseOpportunityFromUserExceptionShowList({ oldCaseCode, pipelineId }) {
    const userPreferences = this.coreService.getUserPreferencesValue();

    if (oldCaseCode && userPreferences) {
      const caseExceptionShowList = userPreferences.caseExceptionShowList ? userPreferences.caseExceptionShowList.split(',') : [];
      if (caseExceptionShowList.includes(oldCaseCode)) {
        caseExceptionShowList.splice(caseExceptionShowList.indexOf(oldCaseCode), 1);
        userPreferences.caseExceptionShowList = caseExceptionShowList.toString();
        this.overlayMessageService.updateUserPreference(userPreferences);
      }
    } else if (pipelineId && userPreferences) {
      const opportunityExceptionShowList = userPreferences.opportunityExceptionShowList ?
        userPreferences.opportunityExceptionShowList.split(',') : [];
      if (opportunityExceptionShowList.includes(pipelineId)) {
        opportunityExceptionShowList.splice(opportunityExceptionShowList.indexOf(pipelineId), 1);
        userPreferences.opportunityExceptionShowList = opportunityExceptionShowList.toString();
        this.overlayMessageService.updateUserPreference(userPreferences);
      }
    }

    userPreferences.caseOppSortOrder = this.removeCaseOppToUserPreferenceSortOrder(oldCaseCode ?? pipelineId);
    this.overlayMessageService.updateUserPreference(userPreferences);

    this.projectAllocationsStore.dispatch(
      new ProjectOverlayActions.UpdateUserPreferencesForPinOrHide({ 
        oldCaseCode,
        pipelineId,
        notifyMessage: 'unpinned successfully',
        userPreferences,
        action: 'pin' 
      })
    )
    //this.upsertUserPreferences(oldCaseCode, pipelineId, 'unpinned successfully', userPreferences);

    return userPreferences;
  }

  addCaseOpportunityToUserExceptionHideList({ oldCaseCode, pipelineId }) {
    const userPreferences = this.coreService.getUserPreferencesValue();

    if (!this.projectExistsInExceptionShowList(oldCaseCode, pipelineId, userPreferences)) {
      if (oldCaseCode && userPreferences) {
        const caseExceptionHideList = userPreferences.caseExceptionHideList ? userPreferences.caseExceptionHideList.split(',') : [];
        if (!caseExceptionHideList.includes(oldCaseCode)) {
          caseExceptionHideList.push(oldCaseCode);
          userPreferences.caseExceptionHideList = caseExceptionHideList.toString();
        }
      } else if (pipelineId && userPreferences) {
        const opportunityExceptionHideList = userPreferences.opportunityExceptionHideList ?
          userPreferences.opportunityExceptionHideList.split(',') : [];
        if (!opportunityExceptionHideList.includes(pipelineId)) {
          opportunityExceptionHideList.push(pipelineId);
          userPreferences.opportunityExceptionHideList = opportunityExceptionHideList.toString();
        }
      }

      this.overlayMessageService.updateUserPreference(userPreferences);
      this.overlayMessageService.updateCaseOpporunityOject(oldCaseCode, null);

      this.projectAllocationsStore.dispatch(
        new ProjectOverlayActions.UpdateUserPreferencesForPinOrHide({ 
          oldCaseCode,
          pipelineId,
          notifyMessage: 'hidden successfully',
          userPreferences,
          action: 'hide' 
        })
      )
      //this.upsertUserPreferences(oldCaseCode, pipelineId, 'hidden successfully', userPreferences);

    }

    return userPreferences;
  }

  removeCaseOpportunityFromUserExceptionHideList({ oldCaseCode, pipelineId }, updateDb) {

    const userPreferences = this.coreService.getUserPreferencesValue();

    if (oldCaseCode) {
      const caseExceptionHideList = userPreferences.caseExceptionHideList ? userPreferences.caseExceptionHideList.split(',') : [];
      if (caseExceptionHideList.includes(oldCaseCode)) {
        caseExceptionHideList.splice(caseExceptionHideList.indexOf(oldCaseCode), 1);
        userPreferences.caseExceptionHideList = caseExceptionHideList.toString();
      }
    } else if (pipelineId) {
      const opportunityExceptionHideList = userPreferences.opportunityExceptionHideList ?
        userPreferences.opportunityExceptionHideList.split(',') : [];
      if (opportunityExceptionHideList.includes(pipelineId)) {
        opportunityExceptionHideList.splice(opportunityExceptionHideList.indexOf(pipelineId), 1);
        userPreferences.opportunityExceptionHideList = opportunityExceptionHideList.toString();
      }
    }

    this.overlayMessageService.updateUserPreference(userPreferences);

    if (updateDb) {
      this.projectAllocationsStore.dispatch(
        new ProjectOverlayActions.UpdateUserPreferencesForPinOrHide({ 
          oldCaseCode,
          pipelineId,
          notifyMessage: 'unhidden successfully',
          userPreferences,
          action: 'hide' 
        })
      )
      //this.upsertUserPreferences(oldCaseCode, pipelineId, 'unhidden successfully', userPreferences);
    }

    return userPreferences;

  }

  upsertUserPreferencesSupplyGroupsWithSharedInfo(userPreferencesSupplyGroups: UserPreferenceSupplyGroup[]){
    this.userPreferencesService.upsertUserPreferencesSupplyGroupsWithSharedInfo(userPreferencesSupplyGroups)
    .subscribe(upsertedData => {
      this.notifyService.showSuccess('Supply Group Saved successfully');
      this.coreService.setUserPreferenceSupplyGroupsInLocalStorage(upsertedData);
      this.userPreferencesMessageService.triggerUserPreferencesSupplyGroupsRefresh(upsertedData);
    });
  }

  // -----------------------Private Function--------------------------------------------//  

  private addCaseOppToUserPreferenceSortOrder(caseOppId) {
    const userPreferences = this.coreService.getUserPreferencesValue();
    const caseOppSortOrder = userPreferences.caseOppSortOrder ? userPreferences.caseOppSortOrder.split(',') : [];
    caseOppSortOrder.push(caseOppId);
    return caseOppSortOrder.toString();
  }

  private removeCaseOppToUserPreferenceSortOrder(caseOppId) {
    const userPreferences = this.coreService.getUserPreferencesValue();
    const caseOppSortOrder = userPreferences.caseOppSortOrder ? userPreferences.caseOppSortOrder.split(',') : [];
    caseOppSortOrder.splice(caseOppSortOrder.indexOf(caseOppId), 1);
    return caseOppSortOrder.toString();
  }

  private projectExistsInExceptionShowList(oldCaseCode, pipelineId, userPreferences: UserPreferences) {
    if (oldCaseCode) {
      return (userPreferences.caseExceptionShowList ?
        userPreferences.caseExceptionShowList.split(',') : []).includes(oldCaseCode);
    } else if (pipelineId) {
      return (userPreferences.opportunityExceptionShowList ?
        userPreferences.opportunityExceptionShowList.split(',') : []).includes(pipelineId);
    }
    return false;
  }

  private upsertUserPreferences(oldCaseCode, pipelineId, notificationText, userPreferences) {
    if (userPreferences.employeeCode) {
      this.coreService.updateUserPreferences(userPreferences)
        .pipe(takeUntil(this.destroy$))
        .subscribe(
          (data) => {
            this.coreService.setUserPreferences(data, false);
            if (oldCaseCode) {
              this.notifyService.showSuccess(`Case ${notificationText}`);
            } else if (pipelineId) {
              this.notifyService.showSuccess(`Opportunity ${notificationText}`);
            }

            this.overlayMessageService.triggerCaseAndOpportunityRefresh();
          },
          (error) => {
            this.notifyService.showError('Error while removing');
          }
        );
    } else {
      this.coreService.insertUserPreferences(userPreferences)
        .pipe(takeUntil(this.destroy$))
        .subscribe(
          (data) => {
            // we want to save employee code from inserted data without refreshing projects and resources
            this.coreService.setUserPreferences(data, false);
            if (oldCaseCode) {
              this.notifyService.showSuccess(`Case ${notificationText}`);
            } else if (pipelineId) {
              this.notifyService.showSuccess(`Opportunity ${notificationText}`);
            }

            this.overlayMessageService.triggerCaseAndOpportunityRefresh();

          },
          (error) => {
            this.notifyService.showError('Error while removing');
          }
        );
    }
  }
}
