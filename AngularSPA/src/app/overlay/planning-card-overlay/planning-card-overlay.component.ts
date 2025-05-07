import { Component, Inject, OnInit, HostListener, Output, EventEmitter, OnDestroy, ElementRef } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Router, RouterEvent, NavigationStart, ActivatedRoute } from "@angular/router";
import { filter } from "rxjs/operators";
import { BsModalService, BsModalRef } from "ngx-bootstrap/modal";
import { SystemconfirmationFormComponent } from "src/app/shared/systemconfirmation-form/systemconfirmation-form.component";
import { GanttService } from "../gantt/gantt.service";
import { NotificationService } from "src/app/shared/notification.service";
import { ValidationService } from "src/app/shared/validationService";
import { LocalStorageService } from "src/app/shared/local-storage.service";
import { DateService } from "src/app/shared/dateService";
import { UserPreferences } from "src/app/shared/interfaces/userPreferences.interface";
import { ConstantsMaster } from "src/app/shared/constants/constantsMaster";
import { OverlayService } from "../overlay.service";
import { ResourceAllocationService } from "src/app/shared/services/resourceAllocation.service";
import { ResourceAllocation } from "src/app/shared/interfaces/resourceAllocation.interface";
import { GridOptions, GridSizeChangedEvent } from "ag-grid-community";
import { CoreService } from "src/app/core/core.service";
import { InvestmentCategory } from "src/app/shared/interfaces/investmentCateogry.interface";
import { CaseRoleType } from "src/app/shared/interfaces/caseRoleType.interface";
import * as moment from "moment";
import { Observable, Subscription } from "rxjs";
import { OverlayMessageService } from "../behavioralSubjectService/overlayMessage.service";
import { UpdateDateFormComponent } from "src/app/shared/update-date-form/update-date-form.component";
import { CommonService } from "src/app/shared/commonService";
import { PlaceholderAllocation } from "src/app/shared/interfaces/placeholderAllocation.interface";
import { UrlService } from "src/app/core/services/url.service";
import { ShowQuickPeekDialogService } from "../dialogHelperService/show-quick-peek-dialog.service";
import { PlanningCard } from "src/app/shared/interfaces/planningCard.interface";
import { PegOpportunityDialogService } from "../dialogHelperService/peg-opportunity-dialog.service";
import * as fromPlanningCardStore from 'src/app/state/reducers/planning-card-overlay.reducer';
import * as PlanningCardOverlayActions from 'src/app/state/actions/planning-card-overlay.action';
import { Store } from "@ngrx/store";
import { SignalrService } from "src/app/shared/signalR.service";
import { PegOpportunity } from "src/app/shared/interfaces/pegOpportunity.interface";

@Component({
    selector: "app-planning-card-overlay",
    templateUrl: "./planning-card-overlay.component.html",
    styleUrls: ["./planning-card-overlay.component.scss"]
})
export class PlanningCardOverlayComponent implements OnInit, OnDestroy {
    // For dragging
    public grabber: boolean = false;
    public height: number = 230;
    public oldYCoord: number = 0;
    public yCoord: number = 100;
    public planningCard: any;
    public showDialog: boolean;
    public openedTab: String;
    public today = new Date();
    public calendarRadioSelected: string = ConstantsMaster.ganttCalendarScaleOptions.defaultSelection;
    private investmentCategories: InvestmentCategory[];
    public activeResourcesEmailAddresses = "";
    private routerSubscription: any;
    public unconfirmedPlaceholderllocations: ResourceAllocation[];
    public confirmedPlaceholderllocations: ResourceAllocation[];
    public activeResourcesStartDate;
    showPlanningCardIntake = false;
    subscription: Subscription = new Subscription();
    appScreens: any;
    accessibleFeatures = ConstantsMaster.appScreens.feature;
      isGanttCalendarReadOnly = false;
       dateMessage = "Does not exist";
       bsModalRef: BsModalRef;
       scrollDistance: number; // how much percentage the scroll event should fire ( 2 means (100 - 2*10) = 80%)
       pageSize: number;
       isPegPlanningCard =  false;

    // -----------------------Output Events--------------------------------------------//

    @Output() openResourceDetailsFromProjectDialog = new EventEmitter();
    @Output() updateResourceAssignmentToProject = new EventEmitter();
    @Output() upsertResourceAllocationsToProject = new EventEmitter<any>();
    @Output() upsertPlaceholderAllocationsToProject = new EventEmitter<any>();
    @Output() deleteResourceFromProject = new EventEmitter();
    @Output() deleteResourcesFromProject = new EventEmitter();
    @Output() deletePlaceholdersFromProject = new EventEmitter();
    @Output() openQuickAddForm = new EventEmitter();
    @Output() insertSkuCaseTermsForProject = new EventEmitter();
    @Output() updateSkuCaseTermsForProject = new EventEmitter();
    @Output() deleteSkuCaseTermsForProject = new EventEmitter();
    @Output() openNotesDialog = new EventEmitter();
    @Output() openSplitAllocationDialog = new EventEmitter();
    @Output() upsertPlaceholderEmitter = new EventEmitter();
    @Output() removePlanningCardEmitter = new EventEmitter();
    @Output() openPlaceholderForm = new EventEmitter();
    @Output() openSTACommitmentForm = new EventEmitter();
    @Output() openSharePlanningCardDialog = new EventEmitter();
    @Output() updatePlanningCard = new EventEmitter();

    constructor(
        public dialogRef: MatDialogRef<PlanningCardOverlayComponent>,
        @Inject(MAT_DIALOG_DATA) public data: any,
        private _overlayService: OverlayService,
        private modalService: BsModalService,
        private _ganttService: GanttService,
        private _notifyService: NotificationService,
        private localStorageService: LocalStorageService,
        private router: Router,
        private route: ActivatedRoute,
        private _resourceAllocationService: ResourceAllocationService,
        private coreService: CoreService,
        private overlayMessageService: OverlayMessageService,
        private urlService: UrlService,
        private elementRef: ElementRef,
        private showQuickPeekDialogService: ShowQuickPeekDialogService,
        private pegOpportunityDialogService: PegOpportunityDialogService,
        private signalrService: SignalrService,
        private planningCardStore: Store<fromPlanningCardStore.State>
    ) {
        


        this.isGanttCalendarReadOnly = CommonService.isReadOnlyAccessToFeature(
            this.accessibleFeatures.caseOverlayGantt,
            this.coreService.loggedInUserClaims.FeatureAccess
        );

        this.investmentCategories = this.localStorageService.get(ConstantsMaster.localStorageKeys.investmentCategories);

        this.activeResourcesStartDate = new Date();
        this.activeResourcesStartDate.setDate(
            this.activeResourcesStartDate.getDate() - ConstantsMaster.ganttConstants.initialDaysDeduction
        );

        //emitting null value to prevent emitting last value
        this._ganttService.resourceAndPlaceholderAllocations.next(null);
        this.dialogRef.disableClose = true; // dialogRef property set to disable dialogBox from closing using esc button.
        this.showDialog = this.data.showDialog;

         this.getPlanningCardDetails(this.data.planningCardId);
        // this.setProjectActions(
        //     this.data.planningCardDetails.oldCaseCode || this.mockOldCaseCode,
        //     this.data.planningCardDetails.pipelineId || null
        // );

        this.subscription.add(this.signalrService.onPegDataUpdated().subscribe((updatedPegPlanningCardData : PegOpportunity) => {
            this.planningCardStore.dispatch(
              new PlanningCardOverlayActions.RefreshPlanningCardOverlaySuccess({
                planningCardDialogRef: this.dialogRef,
                updatedPegPlanningCardData: updatedPegPlanningCardData 
              })
            );
        }));
    }

    getHeight() {
        return {
            height: `calc(100% - ${this.height}px)`
        };
    }

    @HostListener("window:mousemove", ["$event"])
    onMousemove(event: MouseEvent) {
        document.querySelector(".gantt_tooltip")?.remove();
    }

    getDatePicker() {
        function Datepicker() {}
        Datepicker.prototype.init = function (params) {
            this.eInput = document.createElement("input");
            this.eInput.id = "datePicker";
            this.eInput.type = "date";
            this.eInput.classList.add("aggrid-datePicker");
            this.eInput.value = DateService.formatDate(params.value);
            this.eInput.style.height = "100%";
            this.eInput.setAttribute("data-date", params.value);
            this.eInput.setAttribute("data-date-format", "DD-MMM-YYYY");
            this.eInput.onchange = function () {
                const el = <HTMLInputElement>document.getElementById("datePicker");
                const formattedDate = moment(el.value, "YYYY-MM-DD").format(el.getAttribute("data-date-format"));
                el.setAttribute("data-date", formattedDate);
            };
        };
        Datepicker.prototype.getGui = function () {
            return this.eInput;
        };
        Datepicker.prototype.afterGuiAttached = function () {
            this.eInput.focus();
            this.eInput.select();
        };
        Datepicker.prototype.getValue = function () {
            return DateService.convertDateInBainFormat(this.eInput.value);
        };
        Datepicker.prototype.destroy = function () {};
        Datepicker.prototype.isPopup = function () {
            return true;
        };
        return Datepicker;
    }

    // Added to provide animation to dialog box while closing using esc button.
    @HostListener("document:keydown", ["$event"])
    public handleKeyboardEvent(event: KeyboardEvent): void {
        // if-else condition added to close only resource popup when both resource popup and project detail popup are open
        // and user presses esc button.
        if (event.key === "Escape") {
            if ((<HTMLInputElement>event.target).childElementCount > 0) {
                if (
                    (<HTMLInputElement>event.target).firstElementChild.firstElementChild &&
                    ((<HTMLInputElement>event.target).firstElementChild.firstElementChild.id ===
                        "planning-overlay-card" ||
                        (<HTMLInputElement>event.target).firstElementChild.firstElementChild.id === "")
                ) {
                    this.closeDialog();
                }
            } else {
                this.closeDialog();
            }
        }
    }

    // Component Lifecycle Events and Functions
    ngOnInit(): void {
        this.routerSubscription = (this.router.events as Observable<RouterEvent>)
            .pipe(
                filter((event: RouterEvent) => event instanceof NavigationStart),
                filter(() => !!this.dialogRef)
            )
            .subscribe(() => {
                this.dialogRef.close();
            });
        this.openedTab = "calendar";
        this.pageSize = this.coreService.appSettings.resourcesPerPage;
        this.scrollDistance = this.coreService.appSettings.scrollDistance;
        this.coreService.setShowHideNotification(false);
        this.appScreens = ConstantsMaster.appScreens;
    }

    ngAfterViewInit() {
        // Event Listener for dragging and expanding column Div
        this.elementRef.nativeElement.querySelector("#gr").addEventListener("mousedown", this.onMouseDown.bind(this));
    }

    // On mouse move up or down
    @HostListener("document:mousemove", ["$event"])
    onMouseMove(event: MouseEvent) {
        if (!this.grabber) {
            return;
        } else {
            this.resizer(event.clientY - this.oldYCoord);
            this.oldYCoord = event.clientY;
        }
    }

    // Set height variable assigned to columns container to offsetY value
    resizer(offsetY: number) {
        this.height += offsetY;
    }

    // When mouse click is released
    @HostListener("document:mouseup", ["$event"])
    onMouseUp() {
        this.grabber = false;
    }

    // When mouse is clicked down
    onMouseDown(event: MouseEvent) {
        this.grabber = true;
        this.oldYCoord = event.clientY;
    }

    ngOnDestroy(): void {
        this.routerSubscription.unsubscribe();
        this.subscription.unsubscribe();
    }

    changeCalendarOptionHandler(changedCalendarOption) {
        this.calendarRadioSelected = changedCalendarOption;
    }

    // // ------------------------Service/API calls-------------------------------------------------------------------//

    getPlanningCardDetails(planningCardId) {
        if(planningCardId)
        {
            this.getPlanningCardInfoForHeader(planningCardId);
        }
    }

    

    private assignAllocationToProject(allocations: ResourceAllocation[]): void {
        this.confirmedPlaceholderllocations = allocations?.filter(
            (allocation) => !allocation.isPlaceholderAllocation
        );
        this.planningCard.regularAllocations = this.confirmedPlaceholderllocations;
        this.unconfirmedPlaceholderllocations = allocations?.filter(
            (allocation) =>
                allocation.isPlaceholderAllocation &&
                allocation.serviceLineCode &&
                allocation.operatingOfficeCode &&
                allocation.currentLevelGrade
        );
        this.planningCard.placeholderAllocations = this.unconfirmedPlaceholderllocations;
        this.planningCard.allocations = allocations;

        this.confirmedPlaceholderllocations.forEach((allocatedResource) => {
            this.addActiveResourceToEmailString(allocatedResource);
        });

        this.convertDateToBainFormat(this.confirmedPlaceholderllocations);
        this.convertDateToBainFormat(this.unconfirmedPlaceholderllocations);

        this.confirmedPlaceholderllocations = this.sortAllocations(this.confirmedPlaceholderllocations);

        this.confirmedPlaceholderllocations = this.sortAllocations(this.confirmedPlaceholderllocations);
    }
    sortAllocations(allocations) {
        return allocations?.sort((firstAllocation, secondAllocation) => {
            if (firstAllocation.startDate > secondAllocation.startDate) {
                return 1;
            }
            if (firstAllocation.startDate < secondAllocation.startDate) {
                return -1;
            }
            return 0;
        });
    }

    convertDateToBainFormat(allocations) {
        allocations?.forEach((plceholderAllocation) => {
            plceholderAllocation.startDate = DateService.convertDateInBainFormat(plceholderAllocation.startDate);
            plceholderAllocation.endDate = DateService.convertDateInBainFormat(plceholderAllocation.endDate);
        });
    }

    getPlanningCardInfoForHeader(planningCardId) {
        this._overlayService.getPlanningCardDataByPlanningCardId(planningCardId).subscribe((planningCardDetails) => {
            this.planningCard = planningCardDetails[0];   
            if(this.planningCard)
            {
                this.populatePlanningCardGantt(planningCardId, this.activeResourcesStartDate);
            if (this.planningCard.startDate === "0001-01-01T00:00:00") {
                this.planningCard.startDate = this.dateMessage;
            } else {
                this.planningCard.startDate = DateService.convertDateInBainFormat(
                    this.planningCard.startDate
                );
            }
            if (this.planningCard.endDate === "0001-01-01T00:00:00") {
                this.planningCard.endDate = this.dateMessage;
            } else {
                this.planningCard.endDate = DateService.convertDateInBainFormat(
                    this.planningCard.endDate
                );
            }
            }
        });
    }

    addActiveResourceToEmailString(resource) {
        if (
            Date.parse(resource.endDate) >= DateService.getToday() &&
            !this.activeResourcesEmailAddresses.includes(resource.internetAddress)
        ) {
            this.activeResourcesEmailAddresses += resource.internetAddress + ";";
        }
    }

    populatePlanningCardGantt(planningCardId, effectiveDate?) {
        const effectiveFromDate = effectiveDate
            ? DateService.convertDateInBainFormat(effectiveDate)
            : DateService.convertDateInBainFormat(this.today);
        this._overlayService.getPlanningCardAllocations(planningCardId, effectiveFromDate).subscribe((allocations) => {
            this.assignAllocationToProject(allocations);
            var confirmedAndUnconfirmedPlaceholderAllocations = this.unconfirmedPlaceholderllocations.concat(
                this.confirmedPlaceholderllocations
            );
            this._ganttService.resourceAndPlaceholderAllocations.next(confirmedAndUnconfirmedPlaceholderAllocations);
        });
    }

    populateGanttData() {

        this.populatePlanningCardGantt(
            this.data.planningCardId,
            this.activeResourcesStartDate
        );
    }

    // // -------------------Component Event Handlers-------------------------------------//

    closeDialog() {
        this.coreService.setShowHideNotification(false);
        const self = this;

        self.showDialog = false;

        /*
         * Set timeout is required to close the animation with animation and override/delay the defaut closing of material dialog.
         * If not used, the overlay will not close as per our required animation
         * dialogref.close is required to unload the overlay component.
         */
        setTimeout(function () {
            if (
                self.router.url.toLowerCase().includes("/overlay?planningcardid")
            ) {
                this.routeQueryParamSubsciption = self.route.queryParamMap.subscribe((queryParams) => {
                    const reportType = queryParams.get("reportType");
                    switch (reportType) {
                        case "1":
                            self.router.navigate([`/${ConstantsMaster.appScreens.report.staffingAllocation}`]);
                            break;
                        case "2":
                            self.router.navigate([`/${ConstantsMaster.appScreens.report.dailyDeploymentDetails}`]);
                            break;
                        case "3":
                            self.router.navigate([
                                `/${ConstantsMaster.appScreens.report.weeklyDeploymentIndividualResourceDetails}`
                            ]);
                            break;
                        case "4":
                            self.router.navigate([`/${ConstantsMaster.appScreens.report.weeklyDeploymentSummaryView}`]);
                            break;
                        case '5':
                          self.router.navigate(['/analytics/resourceAllocationsToday']);
                          break;
                        case "6":
                            self.router.navigate([`/${ConstantsMaster.appScreens.report.ringfenceStaffing}`]);
                            break;
                        case "12":
                            self.router.navigate([`/${ConstantsMaster.appScreens.report.monthlyDeployment}`]);
                            break;
                        case "13":
                            self.router.navigate([`/${ConstantsMaster.appScreens.report.practiceStaffing}`]);
                            break;
                        default:
                            self.router.navigate([self.urlService.redirectURL]);
                    }
                    self.dialogRef.close();
                });
                this.routeQueryParamSubsciption.unsubscribe();
            } else {
                self.dialogRef.close();
            }
        }, 1000);
    }

    upsertResourceAllocationsToProjectHandler(upsertedAllocations) {
        this.upsertResourceAllocationsToProject.emit(upsertedAllocations);
    }

    upsertPlaceholderAllocationsToProjectHandler(upsertedAllocations) {
        this.upsertPlaceholderAllocationsToProject.emit(upsertedAllocations);
    }

    openSplitAllocationDialogHandler(data) {
        this.openSplitAllocationDialog.emit({ allocationData: data });
    }

    removePlanningCardEmitterHandler(event) {
        this.planningCardStore.dispatch(
            new PlanningCardOverlayActions.DeletePlanningCard({ planningCardId : event.id, planningCardDialogRef :this.dialogRef })
            );
        this.closeDialog();
    }

    isPegPlanningCardEmitterHandler(event) {    
        this.isPegPlanningCard = event;    
    }

    openPegRFPopUpHandler(pegOpportunityId) {
        this.pegOpportunityDialogService.openPegOpportunityDetailPopUp(pegOpportunityId);
      }

    updateDateForSelectedAllocationsHandler(upsertedAllocations) {
        if (
            upsertedAllocations &&
            upsertedAllocations.selectedAllocations &&
            upsertedAllocations.updatedDate &&
            upsertedAllocations.title
        ) {
            const updatedAllocations = this._resourceAllocationService.updateDateForSelectedAllocations(
                upsertedAllocations.selectedAllocations,
                upsertedAllocations.updatedDate,
                upsertedAllocations.title
            );

            const [isValidAllocation, errorMessage] = this._resourceAllocationService.validateMonthCloseForUpdates(
                updatedAllocations,
                upsertedAllocations.selectedAllocations
            );

            if (!isValidAllocation) {
                this._notifyService.showValidationMsg(errorMessage);
                return;
            } else {
                const updatedAllocationsWithPrePost =
                    this._resourceAllocationService.checkAndSplitAllocationsForPrePostRevenue(updatedAllocations);

                this.upsertResourceAllocationsToProjectHandler({
                    resourceAllocation: updatedAllocationsWithPrePost,
                    event: "updateDatePopUp",
                    showMoreThanYearWarning: false
                });
            }
        }
    }


    openSystemConfirmationToDeleteSelectedPlaceholders(event) {
        const projectName =  this.planningCard.name;
        const confirmationPopUpBodyMessage =
            event.confirmationPopUpBodyMessage +
            projectName +
            ". Are you sure you want to remove selected placeholder(s) ?";

        // class is required to center align the modal on large screens
        const config = {
            class: "modal-dialog-centered",

            ignoreBackdropClick: true,
            initialState: {
                confirmationPopUpBodyMessage: confirmationPopUpBodyMessage,
                placeholderIds: event.placeholderIds,
                placeholderAllocation: event.placeholderAllocations
            }
        };
        this.bsModalRef = this.modalService.show(SystemconfirmationFormComponent, config);
        this.bsModalRef.content.deletePlaceholdersFromProject.subscribe((placeholderData) => {
            this.deletePlaceholdersFromProject.emit(placeholderData);
        });
    }

    openUpdateDateForm(event) {
        const config = {
            class: "modal-dialog-centered",
            ignoreBackdropClick: true,
            initialState: {
                resourceAllocations: event.resourceAllocations,
                title: event.title,
                placeholderAllocations: event.placeholderAllocations
            }
        };
        this.bsModalRef = this.modalService.show(UpdateDateFormComponent, config);

        this.bsModalRef.content.updateDataForSelectedAllocations.subscribe((upsertData) => {
            this.updateDateForSelectedAllocationsHandler(upsertData);
        });

        this.bsModalRef.content.updateDataForSelectedPlaceholders.subscribe((upsertData) => {
            this.upsertPlaceholderAllocationsToProjectHandler(upsertData);
        });
    }

    setProjectDataInUpsertedCommitment(resourceAllocationObj: ResourceAllocation): ResourceAllocation {
        resourceAllocationObj.planningCardId =
            resourceAllocationObj.planningCardId ||
            this.planningCard.id;
        resourceAllocationObj.planningCardTitle =
            resourceAllocationObj.planningCardTitle || this.planningCard.name;
        resourceAllocationObj.startDate =
            resourceAllocationObj.startDate ||
            (this.planningCard.id
                ? this.planningCard.startDate
                : null);
        resourceAllocationObj.endDate =
        (this.planningCard.id
            ? this.planningCard.endDate
            : null);

        return resourceAllocationObj;
    }

    openQuickAddFormHandler(modalData) {
        modalData.resourceAllocationData = this.setProjectDataInUpsertedCommitment(modalData.resourceAllocationData);

        this.openQuickAddForm.emit(modalData);
    }
    
    openPlaceholderFormhandler(event) {
        this.openPlaceholderForm.emit({
            planningCardData: this.planningCard,
            placeholderAllocationData: event?.placeholderAllocationData,
            isUpdateModal: event?.isUpdateModal
        });
    }

    onOpenSTACommitmentForm(event) {
     this.openSTACommitmentForm.emit({project:this.planningCard});
    }

    sharePlanningCardEmitterHandler() {
        this.openSharePlanningCardDialog.emit({
            planningCard : this.planningCard,
            isPegPlanningCard : this.isPegPlanningCard
        });
    }

    updatePlanningCardEmitterHandler(event){
        this.updatePlanningCard.emit({
            planningCard : event
        });
    }

    addResource() {
        const resourceAllocationData = this.setProjectDataInUpsertedCommitment({} as ResourceAllocation);
        this.openQuickAddForm.emit({
            commitmentTypeCode: "PC",
            resourceAllocationData: resourceAllocationData,
            isUpdateModal: false
        });
    }

    resourceCodeToOpenHandler(resourceCodeToOpen) {
        this.openResourceDetailsFromProjectDialog.emit(resourceCodeToOpen.resourceCode);
    }

    deleteSelectedPlaceholdersConfirmation(placeholdersData: PlaceholderAllocation | PlaceholderAllocation[]) {
        const placeholderAllocations = [].concat(placeholdersData);
        const selectedIds = placeholderAllocations.map((elem) => elem.id).join(",");
        const confirmationPopUpBodyMessage = "You are about to remove selected placeholder(s) from project ";
        this.openSystemConfirmationToDeleteSelectedPlaceholders({
            placeholderIds: selectedIds,
            placeholderAllocations: placeholderAllocations,
            confirmationPopUpBodyMessage: confirmationPopUpBodyMessage
        });
    }

    getActiveResourcesForProjectOnOrAfterSelectedDateHandler(event) {
        this.activeResourcesStartDate = event.selectedDate;
        if (this.planningCard.id) {
            this.populatePlanningCardGantt(
                this.planningCard.id,
                this.activeResourcesStartDate
            );
        }
    }
}
