import { Component, ElementRef, OnInit, ViewChild, OnDestroy, Output, EventEmitter } from '@angular/core';
import { select, Store } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { CoreService } from 'src/app/core/core.service';
import { SecurityUserDetail } from 'src/app/shared/interfaces/securityUserDetail';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { OfficeHierarchy } from 'src/app/shared/interfaces/officeHierarchy.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';
import { LevelGrade } from 'src/app/shared/interfaces/levelGrade.interface';
import { ServiceLineHierarchy } from 'src/app/shared/interfaces/serviceLineHierarchy';
import { PracticeArea } from 'src/app/shared/interfaces/practiceArea.interface';
import { PositionHierarchy } from 'src/app/shared/interfaces/positionHierarchy.interface';
// --------------------------Redux Component -----------------------------------------//
import * as adminActions from '../State/admin.actions';
import * as fromAdmin from '../State/admin.selectors';
import * as adminState from '../State/admin.state';
import { DateService } from 'src/app/shared/dateService';
import { GridOptions, } from 'ag-grid-enterprise';
import {  FirstDataRenderedEvent, RowDataTransaction} from 'ag-grid-community';
import { AgGridMultiSelectComponent } from 'src/app/shared/ag-grid-multi-select/ag-grid-multi-select.component';
import { BossSecurityRole, ServiceLine } from 'src/app/shared/constants/enumMaster';
import { Office } from 'src/app/shared/interfaces/office.interface';
import { Position } from 'src/app/shared/interfaces/position.interface';
import { AgGridOfficeDropdownComponent } from 'src/app/shared/ag-grid-office-dropdown/ag-grid-office-dropdown.component';
import { CheckboxRenderer } from "src/app/shared/ag-grid-checkbox-renderer/checkbox-renderer.component";
import { ValidationService } from 'src/app/shared/validationService';
import { SecurityRole } from 'src/app/shared/interfaces/securityRole.interface';
import { UserPersonaType } from 'src/app/shared/interfaces/userPersonaType';
import { SecurityGroup } from 'src/app/shared/interfaces/securityGroup';
import { NotificationService } from 'src/app/shared/notification.service';

@Component({
  selector: 'app-users-list',
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.scss']
})
export class UserListComponent implements OnInit, OnDestroy {
  //-------------------Output Events---------------------------------
  @Output() deleteUserEmitter = new EventEmitter();

  //-------------------Local Variables---------------------------------

  public securityUsersDetails;
  public filteredUsers;
  public storeSub: Subscription = new Subscription();
  public showProgressBar = false;
  ringFences: CommitmentType[] = [];
  officeHierarchy: OfficeHierarchy;
  officesFlat: Office[];
  levelGrades: LevelGrade[] = [];
  serviceLines: ServiceLineHierarchy[] = [];
  serviceLinesFlat: any[] = [];
  practiceAreas: PracticeArea[] = [];
  positionsHierarchy: PositionHierarchy[] = [];
  positionsList: Position[] = [];
  userTypeList: UserPersonaType[] = [];
  geoTypelist = ConstantsMaster.UserGeotype;
  securityRoles: SecurityRole[] = [];
  visibleSecurityRoleIds: number[] = [1,2,3,10,11];
  
  public defaultRoleCode = ConstantsMaster.roleCodeForFullBOSS;

  // AG Grid
  public components;
  public frameworkComponents;
  public rowData: any[] = [];
  // public rowData: any[] | null = immutableStore;
  public columnDefs;
  public gridApi;
  public gridColumnApi;
  public defaultColDef;
  public localCellStyles = {
    "color": "#000",
    "font-size": "12px",
  };

  //-----------------------------Life Cycle Events--------------------------------

  constructor(private coreService: CoreService,
    private store: Store<adminState.State>,
    private localStorageService: LocalStorageService,
   private notificationService: NotificationService) {
  }

  ngOnInit() {
    this.getLookUpdataFromLocalStorage();
    this.filterAndUpdateServiceLineHierarchy();
    this.loadGridConfigurations();
    this.setStoreSubscription();
    this.getSecurityUsersDetails();
  }

  //------------------------------AG - Grid Methods----------------------------
  onFirstDataRendered(params: FirstDataRenderedEvent) {
    params.api.sizeColumnsToFit();
  }

  onGridReady(params) {
    this.gridApi = params.api;
    this.gridColumnApi = params.columnApi;
  }
  
  onAgGridCellClick($event){
    if ($event.colDef.field === 'Action') {
      this.deleteSecurityUser($event.data);
    }
  }

  onCellValueChanged(params) {
    if (!(params.newValue === params.oldValue)) {
      const data = params.data;
      this.updateSecurityUser(data);
    }
  }

  gridOptions: GridOptions ={
     context: {
       componentParent: this,
     },
     rowClassRules: {
      'terminated': function(params) { return params.data.isTerminated; },
  },

    components:{
      agEmployeeEditor: AgGridMultiSelectComponent
    },

    getRowId: (params) => params.data.employeeCode,

  };

  // Function to load set AG Grid options
  loadGridConfigurations() {

    const defaultRoleCode = this.defaultRoleCode;

    this.defaultColDef = {

      cellStyle: this.localCellStyles,
      sortable: true, // can sort by client, project name, etc
      filter: true, // can search for case
      editable: true,
      resizable: true,
    };

    this.columnDefs = [
      {
        headerName:"Name",
        field: "fullName",
        minWidth: 150,
        editable: false,
        valueGetter: function (params) {

        let fullName=params.data.fullName;
          if (params.data.isTerminated) {
            fullName += ' (Alumni)'
          }
          return fullName;
        }
      },
      {
        headerName:"Office",
        field: "managingOfficeName",
        minWidth: 35,
        editable: false,
      },
      {
        headerName:"Title",
        field: "jobTitle",
        minWidth: 150,
        editable: false,
      },
      {
        headerName: 'Boss User Type', 
        field: 'userTypeCode', 
        minWidth: 150,
        cellEditor:AgGridMultiSelectComponent,
        cellEditorParams: {
          cellHeight: 35,
          values: this.userTypeList,
          dropdownTitle : 'BOSS User Types',
        },
        valueGetter: function (params) {

          if(params.data.userTypeCode){
            const userTypeCode = params.data.userTypeCode;
            const userTypeName= params.context.componentParent.userTypeList.filter(x => userTypeCode.includes(x.userTypeCode)).map(x => x.userTypeName).join(",");
            return userTypeName;

          }
        },
        valueSetter: function (params) {
          if(params.newValue){
            params.data.userTypeCode = params.newValue.join(",");
          }else
          {
            params.data.userTypeCode = '';
          }

          return true;
        },
        tooltipValueGetter: (params) => {
          if (params.data.userTypeCode) {
            const userTypeCode = params.data.userTypeCode;
            return params.context.componentParent.userTypeList
              .filter(x => userTypeCode.includes(x.userTypeCode))
              .map(x => x.userTypeName)
              .join(", ");
          }
          return '';
        }
      },
      
      {
        headerName: 'Staffing Geo Type', field: 'geoType', minWidth: 150,
        cellEditor: 'agRichSelectCellEditor',
        cellEditorParams: {
          cellHeight: 35,
          values: this.geoTypelist.map(geoType =>
            geoType.text),
        }
      },
      {
        headerName: 'Staffing Geo Name',
        field: "officeCodes",
        minWidth: 250,
        cellEditor:AgGridOfficeDropdownComponent,
        cellEditorParams: {
          cellHeight: 45,
          values: this.officeHierarchy,
          appendParentCodesOnSelect: true
        },
        valueGetter: function (params) {
          if(params.data.officeCodes){
            const officeCodes = params.data.officeCodes;
            const officeNames = params.context.componentParent.officesFlat.filter(x => officeCodes.includes(x.officeCode)).map(x => x.officeName).join(",");
            return officeNames;
          }

        },
        valueSetter: function (params) {
          if(params.newValue){
            params.data.officeCodes = params.newValue.join(",");
          }else
          {
            params.data.officeCodes = '';

          return true;
        }
        }
      },
      {
        headerName: 'Staffing Service Line',
        field: "serviceLineCodes",
        minWidth: 210,
        cellEditor:AgGridMultiSelectComponent,
        cellEditorParams: {
          cellHeight: 45,
          values: this.serviceLines,
          dropdownTitle : 'Service Lines',
          dropdownTypeCode : 'H',
          appendParentCodesOnSelect: true
        },
        valueGetter: function (params) {
          if(params.data.serviceLineCodes){
            const serviceLineCodes = params.data.serviceLineCodes;
            const serviceLineNames= params.context.componentParent.serviceLinesFlat.filter(x => serviceLineCodes.includes(x.serviceLineCode)).map(x => x.serviceLineName).join(",");
            return serviceLineNames;

          }
        },
        valueSetter: function (params) {
          if(params.newValue){
            params.data.serviceLineCodes = params.newValue.join(",");
          }else
          {
            params.data.serviceLineCodes = '';
          }

          return true;
        },
        tooltipValueGetter: function (params) {
          if (params.data.serviceLineCodes) {
            const serviceLineCodes = params.data.serviceLineCodes;
            const serviceLineNames = params.context.componentParent.serviceLinesFlat
              .filter(x => serviceLineCodes.includes(x.serviceLineCode))
              .map(x => x.serviceLineName)
              .join(", ");
            return serviceLineNames;
          }
          return '';
        }
      },
      {
        headerName: 'Staffing Position Group',
        field: "positionGroupCodes",
        minWidth: 250,
        cellEditor:AgGridMultiSelectComponent,
        cellEditorParams: {
          cellHeight: 45,
          values: this.positionsHierarchy,
          dropdownTitle : 'Position Groups',
          dropdownTypeCode : 'H'
        },
        valueGetter: function (params) {
          if(params.data.positionGroupCodes){
            const positionGroupCodes = params.data.positionGroupCodes;
            const positionNames = params.context.componentParent.positionsList.filter(x => positionGroupCodes.includes(x.positionCode)).map(x => x.defaultJobTitle).join(",");
            return positionNames;

          }
        },
        valueSetter: function (params) {
          if(params.newValue){
            params.data.positionGroupCodes = params.newValue.join(",");
          }else
          {
            params.data.positionGroupCodes = '';
          }

          return true;
        },
        tooltipValueGetter: (params) => {
          if (params.data.positionGroupCodes) {
            const positionGroupCodes = params.data.positionGroupCodes;
            return params.context.componentParent.positionsList
              .filter(x => positionGroupCodes.includes(x.positionCode))
              .map(x => x.defaultJobTitle)
              .join(", ");
          }
          return '';
        }
      },

      {
        field: "levelGrades",
        headerName:'Staffing Level Grade',
        minWidth: 250,
        cellEditor:AgGridMultiSelectComponent,
        cellEditorParams: {
          cellHeight: 45,
          values: this.levelGrades,
          dropdownTitle : 'Level Grades',
          dropdownTypeCode : 'H'
        },
        valueSetter: function (params) {
          if(params.newValue){
            params.data.levelGrades = params.newValue.join(",");
          }else
          {
            params.data.levelGrades = '';
          }

          return true;
        }
      },
      {
        headerName:'Staffing Practice',
        field: "practiceAreaCodes",
        minWidth: 250,
        cellEditor:AgGridMultiSelectComponent,
        cellEditorParams: {
          cellHeight: 45,
          values: this.practiceAreas,
          dropdownTitle : 'Practice Areas'
        },
        valueGetter: function (params) {

          if(params.data.practiceAreaCodes){
            const practiceAreaCodes = params.data.practiceAreaCodes;
            const practiceAreaNames = params.context.componentParent.practiceAreas?.filter(x => practiceAreaCodes.split(',').includes(x.practiceAreaCode.toString())).map(x => x.practiceAreaName).join(",");
            return practiceAreaNames;
          }

        },
        valueSetter: function (params) {
          if(params.newValue){
            params.data.practiceAreaCodes = params.newValue.join(",");
          }else
          {
            params.data.practiceAreaCodes = '';
          }

          return true;
        }
      },
      {
        headerName:'Staffing Ringfence',
        field: "ringfenceCodes",
        minWidth: 200,
        cellEditor:AgGridMultiSelectComponent,
        cellEditorParams: {
          cellHeight: 45,
          values: this.ringFences,
          dropdownTitle : 'Ringfences'
        },
        valueGetter: function (params) {

          if(params.data.ringfenceCodes){
            const ringfenceCodes = params.data.ringfenceCodes;
            const ringfenceNames = params.context.componentParent.ringFences.filter(x => ringfenceCodes.includes(x.commitmentTypeCode)).map(x => x.commitmentTypeName).join(",");
            return ringfenceNames;
          }

        },
        valueSetter: function (params) {
          if(params.newValue){
            params.data.ringfenceCodes = params.newValue.join(",");
          }else
          {
            params.data.ringfenceCodes = '';
          }

          return true;
        }
      },
      {
        headerName:"Staffing Role", 
        field: "roleCodes", 
        minWidth: 150,
        cellEditor:AgGridMultiSelectComponent,
        cellEditorParams: {
          cellHeight: 35,
          values: this.securityRoles,
          dropdownTitle : 'Staffing Roles',
        },
        valueGetter: function (params) {

          if(params.data.roleCodes){
            const roleCodes = params.data.roleCodes === "" ? defaultRoleCode : params.data.roleCodes;
            const roleNames = params.context.componentParent.securityRoles.filter(x => roleCodes.split(',').includes(x.roleCode.toString())).map(x => x.roleName).join(",");
            return roleNames;
          }
        },
      
        valueSetter: function (params) {
          if(params.newValue){
            params.data.roleCodes = params.newValue.toString()=== "" ? defaultRoleCode : params.newValue.join(",");
          }
          else
          {
            params.data.roleCodes = '';
          }

          return true;
        },

        tooltipValueGetter: function (params) {
          if (params.data.roleCodes) {
            const roleCodes = params.data.roleCodes === "" ? defaultRoleCode : params.data.roleCodes;
            const roleNames = params.context.componentParent.securityRoles
              .filter(x => roleCodes.split(',').includes(x.roleCode.toString()))
              .map(x => x.roleName)
              .join(", ");
            return roleNames;
          }
          return '';
        }
      },
      {
        field: "override",
        maxWidth: 120,
        editable: true,
        sortable: false,
        cellStyle: this.localCellStyles,
        cellRenderer: "agCheckboxCellRenderer",
        menuTabs: ["filterMenuTab"],
      },
      {
        headerName:'AI Search Access',
        field: "hasAccessToAISearch",
        maxWidth: 120,
        editable: true,
        sortable: false,
        cellStyle: this.localCellStyles,
        cellRenderer: "agCheckboxCellRenderer",
        menuTabs: ["filterMenuTab"],
      },
      {
        headerName:'Staffing Insights Access',
        field: "hasAccessToStaffingInsightsTool",
        maxWidth: 160,
        editable: true,
        sortable: false,
        cellStyle: this.localCellStyles,
        cellRenderer: "agCheckboxCellRenderer",
        menuTabs: ["filterMenuTab"],
      },
      {
        headerName:'Retired Staffing Tab Access',
        field: "hasAccessToRetiredStaffingTab",
        maxWidth: 160,
        editable: true,
        sortable: false,
        cellStyle: this.localCellStyles,
        cellRenderer: "agCheckboxCellRenderer",
        menuTabs: ["filterMenuTab"],
      },
      {
        headerName: 'End Date', field: 'endDate', maxWidth: 140,
        editable: true,
        comparator: function (valueA, valueB) {
          return Date.parse(valueA) - Date.parse(valueB);
        },
        cellStyle: this.localCellStyles,
        valueGetter: params => {
          return DateService.convertDateInBainFormat(params.data.endDate);
        },
        valueSetter: function (params) {
          if (params.newValue==="") {
            const updatedData = Object.assign({}, params.data);
            updatedData.endDate = params.newValue;
            params.data.endDate = updatedData.endDate;
            return true;
          }
          if(ValidationService.isValidDate(params.newValue)){
            const updatedData = Object.assign({}, params.data);
            updatedData.endDate = params.newValue;
            params.data.endDate = updatedData.endDate;
            return true;
          }
          return false;
        },
        filter: 'agDateColumnFilter',
        filterParams: {
          applyButton: true,
          clearButton: true,
          comparator: function (valueA, valueB) {
            return Date.parse(valueA) - Date.parse(valueB);
          },
        }
      },
      {
        field: "notes",
        maxWidth: 150,
        cellStyle: this.localCellStyles,
        menuTabs: ["filterMenuTab"],
      },
      {
        headerName: "Modified By",
        field: "lastUpdatedBy",
        editable: false,
        maxWidth: 150,
        menuTabs: ["filterMenuTab"],
      },
      {
        headerName: "Last Modified",
        field: "lastUpdated",
        editable: false,
        sortable: true,
        maxWidth: 150,
        menuTabs: ["filterMenuTab"],
        comparator: function (valueA, valueB) {
          return Date.parse(valueA) - Date.parse(valueB);
        },
        valueGetter: params => {
          return DateService.convertDateInBainFormat(params.data.lastUpdated);
        },
      },
      {
        headerName: '',
        field: "Action",
        maxWidth: 150,
        filter:false,
        editable: false,
        cellRenderer: function () {
          return '<button class="btn btn-sm btn-outline-danger btn-delete ag-cell-delete-btn">Delete</button>';
        }
      },
    ];
    this.frameworkComponents = {
      checkboxRenderer: CheckboxRenderer
    };
  }

  // this will prevent re-rendering of the html table when
  // a row is inserted/updated/deleted
  getUniqueIdentifier(user: SecurityUserDetail) {
    return user.employeeCode;
  }

  //---------------------------Store Subscription-------------------------------------------
  private setStoreSubscription() {
    this.setLoadStaffingUsersSusbscription();
    this.setLoaderSubscription();
  }

  private setLoadStaffingUsersSusbscription() {
    this.storeSub.add(this.store.pipe(
      select(fromAdmin.getStaffingUsers)
    ).subscribe((staffingUsers: SecurityUserDetail[]) => {
      //TODO: hack to prevent auto-sorting on every save

      if(this.filteredUsers?.length){
        //this.filteredUsers = staffingUsers;
      }else{
        this.filteredUsers = this.securityUsersDetails = this.sortSecurityUsersByDate(staffingUsers);
      }


      this.rowData=this.filteredUsers;
      //this.staffingUsersSearchInput.nativeElement.value = '';
    }));

  }

  private setLoaderSubscription() {
    this.storeSub.add(this.store.pipe(
      select(fromAdmin.staffingUsersLoader)
    ).subscribe((staffingUsersLoader: boolean) => {
      this.showProgressBar = staffingUsersLoader;
    }));
  }

  //---------------------------Helper Methods-------------------------------------------
  getLookUpdataFromLocalStorage() {

    this.officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy);
    this.officesFlat = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList);
    this.levelGrades = this.localStorageService.get(ConstantsMaster.localStorageKeys.levelGradesHierarchy);
    this.serviceLines = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLinesHierarchy);
    this.serviceLinesFlat = this.localStorageService.get(ConstantsMaster.localStorageKeys.serviceLines);
    this.positionsHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.positionsHierarchy);
    this.positionsList = this.localStorageService.get(ConstantsMaster.localStorageKeys.positions);
    this.practiceAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.practiceAreas);
    const securityRoles = this.localStorageService.get(ConstantsMaster.localStorageKeys.securityRoles);
    this.securityRoles = securityRoles.filter(x => this.visibleSecurityRoleIds.some(code => code === x.roleCode));
    
    this.userTypeList = this.localStorageService.get(ConstantsMaster.localStorageKeys.userPersonaTypes);
    this.ringFences = this.localStorageService.get(ConstantsMaster.localStorageKeys.ringfences).filter(x => x.commitmentTypeCode !== 'PS'); //exclude PEG Syrge from RF
  }


  filterAndUpdateServiceLineHierarchy(){
    const activeServiceLines = this.serviceLinesFlat.filter(sl => sl.inActive == true);
  
    activeServiceLines.forEach(serviceLine => {
      const hierarchyItem = this.serviceLines.find(h => h.value === serviceLine.serviceLineHierarchyCode);
  
      if (hierarchyItem) {
        hierarchyItem.children?.push({
          text: serviceLine.serviceLineName,
          value: serviceLine.serviceLineCode,
          children: null // No further hierarchy
        });
      } else {
        this.serviceLines.push({
          text: serviceLine.serviceLineHierarchyName,
          value: serviceLine.serviceLineHierarchyCode,
          children: [{
            text: serviceLine.serviceLineName,
            value: serviceLine.serviceLineCode,
            children: null // No further hierarchy
          }]
        });
      }
    });
  
    return;
  }
  // openEditNotesDialog(notes, employeeCode) {
  //   const config = {
  //     class: 'modal-dialog-centered',
  //     ignoreBackdropClick: true,
  //     initialState: {
  //       inputNotes: notes,
  //       uniqueId: employeeCode,
  //       maxLength: 1000
  //     }
  //   };

  //   this.bsModalRef = this.modalService.show(InlineEditableNotesComponent, config);
  //   this.bsModalRef.content.updateNotesEventEmitter.subscribe(data => {
  //     this.updateNotes(data);
  //   });
  // }

  // updateNotes(data) {
  //   let userToUpdate: SecurityUserDetail = this.getUserToUpdate(data.uniqueId);
  //   if (data.updatedNotes !== userToUpdate.notes?.trim()) {
  //     userToUpdate.notes = data.updatedNotes;
  //     this.updateSecurityUser(userToUpdate);
  //   }
  // }
  // private getUserToUpdate(employeeCode) {
  //   return this.filteredUsers.find(emp => emp.employeeCode === employeeCode);
  // }

  deleteSecurityUser(securityUserDetails: SecurityUserDetail) {
    const confirmationPopUpBodyMessage = 'Are you sure you want to delete user "' + securityUserDetails.fullName + '" from the database ?';
    this.deleteUserEmitter.emit({deleteConfirmationMessage: confirmationPopUpBodyMessage, idToDelete: securityUserDetails.employeeCode});
  }

  //---------------------------Methods called directly from parent-------------------------------------------
  public filterUsersBySearchString(searchTextValue) {
    if (searchTextValue.length < 1) {
      this.filteredUsers = this.securityUsersDetails;
    }else{
      this.filteredUsers = this.securityUsersDetails.filter(securityUser => {
        if (!!securityUser.fullName) {
          return (
            securityUser.fullName.toLowerCase().includes(searchTextValue.trim().toLowerCase())
            || securityUser.employeeCode === searchTextValue.trim()
          );
        }
      });
    }

    this.rowData = this.filteredUsers;
  }

  addUserHandler(userData) {

    const addNewStaffingUserReqObj : SecurityUserDetail= {
      employeeCode: userData.user.employeeCode,
      lastUpdatedBy: this.coreService.loggedInUser.fullName,
      lastUpdated: DateService.getBainFormattedToday(),
      serviceLine: userData.user.serviceLine.serviceLineName,
      jobTitle: userData.user.levelName,
      isAdmin: false,
      fullName: userData.user.fullName,
      isTerminated: userData.user.isTerminated,

      roleCodes: userData.roleCodes,
      userTypeCode: userData.userTypeCode,
      geoType: userData.geoType,
      officeCodes: userData.officeCodes,
      serviceLineCodes: userData.serviceLineCodes,
      positionGroupCodes: userData.positionGroupCodes,
      levelGrades: userData.levelGrades,
      practiceAreaCodes: userData.practiceAreaCodes,
      ringfenceCodes: userData.ringfenceCodes,

      override: false,
      notes: userData.notes,
      endDate: userData.endDate,
      hasAccessToAISearch: userData.hasAccessToAISearch,
      hasAccessToStaffingInsightsTool: userData.hasAccessToStaffingInsightsTool,
      hasAccessToRetiredStaffingTab: userData.hasAccessToRetiredStaffingTab
    };

   
    this.addNewStaffingUser(addNewStaffingUserReqObj);
  }

  public upsertPracticeBasedRingfence(praticeBasedRingfence: CommitmentType) {
    this.store.dispatch(
      new adminActions.UpsertPracticeBasedRingfence(praticeBasedRingfence)
    );
  }

  public confirmDeleteSecurityUser(employeeCode: string) {
    this.showHideStaffingUsersLoader(true);
    this.store.dispatch(
      new adminActions.DeleteSaffingUser(employeeCode)
    );

    //this.gridOptions.api.applyTransaction({ remove: employeeCode})

    var rowNode = this.gridApi.getRowNode(employeeCode.toString());

    let index = this.rowData.findIndex(i => i.employeeCode == rowNode.data.employeeCode);
    this.rowData.splice(index, 1);
    this.gridApi.applyTransaction({ remove: [rowNode.data] });

  }

  private sortSecurityUsersByDate(securityUsersDetails: SecurityUserDetail[]) {
    return securityUsersDetails.sort((a, b) => <any>new Date(b.lastUpdated) - <any>new Date(a.lastUpdated));
  }

  private updateSecurityUser(data: SecurityUserDetail) {

    const updateRequestObj = {
      lastUpdatedByName: this.coreService.loggedInUser.fullName,
      employeeCode: data.employeeCode,
      serviceLine: data.serviceLine,
      jobTitle: data.jobTitle,
      fullName: data.fullName,
      isTerminated: data.isTerminated,
      isAdmin: data.isAdmin,
      override: data.override,
      notes: data.notes,
      endDate: data.endDate,
      roleCodes: (data.roleCodes === "" ? this.defaultRoleCode : data.roleCodes) ?? this.defaultRoleCode,
      userTypeCode: data.userTypeCode,
      geoType: data.geoType,
      officeCodes: data.officeCodes,
      serviceLineCodes: data.serviceLineCodes,
      positionGroupCodes: data.positionGroupCodes,
      levelGrades: data.levelGrades,
      practiceAreaCodes: data.practiceAreaCodes,
      ringfenceCodes: data.ringfenceCodes,
      hasAccessToAISearch : data.hasAccessToAISearch,
      hasAccessToStaffingInsightsTool: data.hasAccessToStaffingInsightsTool,
      hasAccessToRetiredStaffingTab: data.hasAccessToRetiredStaffingTab
    };
    
    this.showHideStaffingUsersLoader(true);

    if (
      data.roleCodes?.includes(BossSecurityRole.WFP) &&
      (!data.serviceLineCodes || data.serviceLineCodes.length === 0 || 
       !data.officeCodes || data.officeCodes.length === 0)
    ) {
      this.notificationService.showWarning("Staffing Service Line and Staffing Geo Name fields are mandatory for the WFP users.", "VALIDATION");
      return;
    }

    this.store.dispatch(
      new adminActions.UpsertSecurityUser(updateRequestObj)
    );

    this.gridApi.applyTransaction(this.rowData);
    var rowNode = this.gridApi.getRowNode(updateRequestObj.employeeCode.toString());

    rowNode.data.lastUpdatedBy = this.coreService.loggedInUser.fullName;
    rowNode.data.lastUpdated = DateService.getBainFormattedToday();

    this.gridApi.applyTransaction({ update: [rowNode] });

  }

  //------------------Store Action Dispatchers--------------------------------------------
  private addNewStaffingUser(addNewStaffingUserReqObj: any) {
    this.showHideStaffingUsersLoader(true);
    this.store.dispatch(
      new adminActions.UpsertSecurityUser(addNewStaffingUserReqObj)
    );

    //this.gridOptions.api.applyTransaction({ add: addNewStaffingUserReqObj})

    const res= addNewStaffingUserReqObj;
    this.rowData.unshift(res);
        let row: RowDataTransaction = { add: [res], addIndex: 0 }
        this.gridApi.applyTransaction(row);

  }

  private getSecurityUsersDetails() {
    this.showHideStaffingUsersLoader(true);
    this.store.dispatch(
      new adminActions.LoadStaffingUsers()
    );
  }

  private showHideStaffingUsersLoader(value: boolean) {
    this.store.dispatch(
      new adminActions.ShowHideStaffingUsersLoader(value)
    );
  }


  //---------------------------------Destroy method -------------------------------------------
  ngOnDestroy() {
    this.storeSub.unsubscribe();
  }

}

