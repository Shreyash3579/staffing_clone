import { UserPreferences } from 'src/app/shared/interfaces/userPreferences.interface';
import { ServiceLine } from 'src/app/shared/constants/enumMaster';
import { ConstantsMaster } from '../../shared/constants/constantsMaster';
import { TreeviewItem, TreeviewHelper } from '@soy-andrey-semyonov/ngx-treeview';
import { Observable,BehaviorSubject} from 'rxjs';
import { UserPreferenceSupplyGroup } from 'src/app/shared/interfaces/userPreferenceSupplyGroup';
import { environment } from 'src/environments/environment';
declare var tableau: any;

export class TableauService {
  viz: any;
  workbook: any;
  myCustomView:any;
  activeSheet: any;
  coreServiceSub: any;
  userPreferences: UserPreferences;
  userPreferenceSupplyGroup: any;
  serviceLineEnum: typeof ServiceLine = ServiceLine;
  url: string;
  tableau: any;
  localStorageService: any;
  urlPassThrough: string;
  // Tableau Dashboard Default options
  options = {
    hideTabs: false,
    hideToolbar: false,
    // showShareOptions: false,
    width: window.innerWidth - 50,
    height: window.innerHeight - 100,
    onFirstInteractive: function () {
    }
  };
  taburl: string = '';
  private mytaburl: BehaviorSubject<string> = new BehaviorSubject<string>(this.taburl);
  getmyTaburl: Observable<string> = this.mytaburl.asObservable();
  private readonly userPreferencesLastUpdatedTimestamp;
  private readonly defaultBOSSCustomViewBaseName = 'BossCustomView_';
  
  constructor(url: string, localStorageService: any, urlPassThorugh) {
    this.url = url;
    this.localStorageService = localStorageService;
    this.urlPassThrough = environment.settings.environmentUrl; //directory using it from environment as this remains same for all reports
    this.userPreferencesLastUpdatedTimestamp = this.localStorageService.get(ConstantsMaster.localStorageKeys.userPreferencesLastUpdatedTimestamp);
  }


  public loadTableauDashboard(placeholderDiv, options) {
     // Dispose tableau visualization if already exists to allow load new viz on change of user settings
     var that = this
     this.options = {
       hideTabs: false,
       hideToolbar: false,
      //  showShareOptions: false,
       width: window.innerWidth - 50,
       height: window.innerHeight - 100,
       onFirstInteractive: function () {
         that.workbook = that.viz.getWorkbook();
         that.activeSheet = that.workbook.getActiveSheet();
         that.workbook.getCustomViewsAsync().then(views => {
           let defaultCustomView = null;
           var isUpdatedBossViewExist = false;
           var isBossCustomViewSetAsDeafult = false;

           views.forEach(element => {
             defaultCustomView = element.getName();

             if((defaultCustomView === 'BossCustomView' || defaultCustomView.indexOf(that.defaultBOSSCustomViewBaseName) > -1) && element.getDefault())
              isBossCustomViewSetAsDeafult = true;

             if(defaultCustomView.indexOf(that.defaultBOSSCustomViewBaseName) > -1 ){
              var bossCustomViewCreationTimestamp = new Date(parseInt(defaultCustomView.substring(defaultCustomView.indexOf("_") + 1)));
              
              if(!that.userPreferencesLastUpdatedTimestamp || bossCustomViewCreationTimestamp > new Date(that.userPreferencesLastUpdatedTimestamp)){
                isUpdatedBossViewExist = true;
              }
             }
           });

           var isAnyBossCustomViewExists = false;
           if(!isUpdatedBossViewExist){
            views.forEach(element => {
              const customeViewName = element.getName();
              if(customeViewName.indexOf('BossCustomView') > -1 && isBossCustomViewSetAsDeafult){
                isAnyBossCustomViewExists = true;
                that.workbook.removeCustomViewAsync(customeViewName).then((view) => {
                  //removeCustomViewAsync only works if loadOriginalDashboard is called once the remove call is completed
                  // if loadOriginalDashboard is called before   removeCustomViewAsync is completed then custom view would not be deleted
                  that.loadOriginalDashboard(placeholderDiv, options);
                });
              }
            });

            //create a new custom view as default if no custom views alredy exist
            if(!isAnyBossCustomViewExists){
              that.loadOriginalDashboard(placeholderDiv, options);
            }
            
           }
        });
      }
     }
     this.viz = new tableau.Viz(placeholderDiv, this.url, this.options);
  }
  public loadOriginalDashboard(placeholderDiv, options) {
    var these = this;
    this.options = {
      hideTabs: false,
      hideToolbar: false,
      // showShareOptions: false,
      width: window.innerWidth - 50,
      height: window.innerHeight - 100,
      onFirstInteractive: function () {
        these.workbook = these.viz.getWorkbook();
        these.activeSheet = these.workbook.getActiveSheet();
        var customViewNameWithCreationTimestamp = these.defaultBOSSCustomViewBaseName + new Date().getTime();
        these.workbook.rememberCustomViewAsync(customViewNameWithCreationTimestamp).then(view => {
          these.myCustomView = view;
          these.workbook.setActiveCustomViewAsDefaultAsync();
          
        });
      }
    }
    if (this.viz) {
      this.viz.dispose();
    }
    options.onFirstInteractive = this.options.onFirstInteractive;
    this.options = options;
    this.viz = new tableau.Viz(placeholderDiv, this.url, this.options);
  }

  public callShare(envPageName){
   this.viz.showShareDialog();
   this.viz.getCurrentUrlAsync().then(view => {
      let viewURLWithoutExternalParams = view.split('?')[0]; //external params are added in individual reports
      this.taburl = envPageName +"?tabUrl="+viewURLWithoutExternalParams;
      //Broadcast changes to components
      this.mytaburl.next(this.taburl);
      console.log('>>Viewer',view);
    });
  }
  p
// Add a listener to the viz
public listenToMarksSelection() {
  this.viz.addEventListener(tableau.TableauEventName.MARKS_SELECTION, this.onMarksSelection);
}
// Specify what happens when a mark is selected on the viz
public onMarksSelection(marksEvent) {
	var sheet = marksEvent.getWorksheet();
  var sheetname = sheet.getName();
	return marksEvent.getMarksAsync().then(this.saveCustomView);
}
// Save a Custom View
public saveCustomView()
{
	this.workbook.rememberCustomViewAsync("BossCustomView").then(view => {
    this.saveAsDefault(view);
  });
}
public saveAsDefault(view)
{
	this.myCustomView = view;
	this.workbook.setActiveCustomViewAsDefaultAsync();
}

  public revertAllDefault() {
    var workbook = this.viz.getWorkbook();
    workbook.getCustomViewsAsync().then(views => {
      workbook.showCustomViewAsync().then(data => {
        console.log('Revertedall',data);
        workbook.setActiveCustomViewAsDefaultAsync();
      });
      
    });

    }


  public onResize(placeholderDiv) {
    this.options.width = window.innerWidth - 50;
    this.options.height = window.innerHeight - 120;
    this.loadTableauDashboard(placeholderDiv,this.options);
  }

  public getTableauDashboardDefaultOptions() {
    return this.options;
  }

  public getSelectedFilters(userPreferences: UserPreferences, userPreferenceSupplyGroup: UserPreferenceSupplyGroup){
    if(userPreferences.isDefault){
      return this.getAppSelectedFilters(userPreferences);
    }
    else{
      return this.getSupplyGroupFilters(userPreferenceSupplyGroup);
    }
  }

  public getAppSelectedFilters(userPreferences: UserPreferences) {
    this.userPreferences = userPreferences;
    const officeList = this.localStorageService.get(ConstantsMaster.localStorageKeys.OfficeList) ?? '';
    const officeHierarchy = this.localStorageService.get(ConstantsMaster.localStorageKeys.officeHierarchy) ?? '';
    const staffingTags = this.localStorageService.get(ConstantsMaster.localStorageKeys.staffingTags) ?? '';

    const preferencedStaffingTags = !!this.userPreferences.supplyViewStaffingTags
      ? this.userPreferences.supplyViewStaffingTags
      : this.serviceLineEnum.GeneralConsulting ?? '';
    const preferencedOffices = this.userPreferences.supplyViewOfficeCodes.split(',')?? '';
    const preferencedDateRangeWeeks = this.userPreferences.supplyWeeksThreshold?? '';

    const selectedOfficeRegions = this.getSelectedofficesRegion(officeHierarchy, preferencedOffices)?? '';
    const selectedOfficeClusters = this.getSelectedofficesCluster(officeHierarchy, preferencedOffices)?? '';
    const selectedOfficeAbbreviations = officeList.filter(x => preferencedOffices.includes(x.officeCode.toString()))
      .map(x => x.officeAbbreviation).join(',')?? '';
    const selectedOfficeNames = officeList.filter(x => preferencedOffices.includes(x.officeCode.toString()))
      .map(x => this.escapeCommaInName(x.officeName)).join(',')?? '';
    const selectedStaffingTags = staffingTags.filter(x => preferencedStaffingTags.includes(x.serviceLineCode.toString()))
      .map(x => x.serviceLineName).join(',')?? '';
    const selectedDateRangeWeeks = preferencedDateRangeWeeks?? '';
    const selectLevelGrades = !this.userPreferences.levelGrades ? '' : this.userPreferences.levelGrades?? '';

    return {
      selectedOfficeRegions,
      selectedOfficeClusters,
      selectedOfficeAbbreviations,
      selectedOfficeNames,
      selectedStaffingTags,
      selectedDateRangeWeeks,
      selectLevelGrades,
      urlPassThrough: this.urlPassThrough,
      options: this.options
    };
  }

  public getSupplyGroupFilters(userPreferenceSupplyGroup: UserPreferenceSupplyGroup){
    this.userPreferenceSupplyGroup = userPreferenceSupplyGroup;
    const selectedSupplyGroupEmployees = this.userPreferenceSupplyGroup.find(x=>x.isDefault==true)?.groupMembers.map(obj => {
      return `${this.escapeCommaInName(obj.employeeName)} (${obj.employeeCode})`;
    }).join(',') ?? '';
    return {
      selectedSupplyGroupEmployees,
      urlPassThrough: this.urlPassThrough,
      options: this.options
    }; 
  }

  private getSelectedofficesRegion(officeHierarchy, selectedOffices) {
    const treeViewOfficeitems = [new TreeviewItem(officeHierarchy)];
    const selectedRegions = [];
    selectedOffices.forEach(officeCode => {
      const office = TreeviewHelper.findItem(treeViewOfficeitems[0], officeCode.toString());
      if(office){
        this.findParentItem(treeViewOfficeitems[0], office, selectedRegions);
      }
    });
    return selectedRegions.toString();
  }

  private getSelectedofficesCluster(officeHierarchy, selectedOffices) {
    const treeViewOfficeitems = [new TreeviewItem(officeHierarchy)];
    const selectedOfficeClusters = [];
    selectedOffices.forEach(officeCode => {
      const office = TreeviewHelper.findItem(treeViewOfficeitems[0], officeCode.toString());
      if(office){
      this.findImmediateParentItem(treeViewOfficeitems[0], office, selectedOfficeClusters);
      }
    });
    return selectedOfficeClusters.toString();
  }

  private findImmediateParentItem(treeViewOfficeitems, office, selectedOfficeClusters) {
    const parentOffice = TreeviewHelper.findParent(treeViewOfficeitems, office);
    if (!selectedOfficeClusters.some(x => x === parentOffice?.text)) {
      selectedOfficeClusters.push(parentOffice.text);
    }
  }

  private findParentItem(treeViewOfficeitems, office, selectedRegions) {
    const parentOffice = TreeviewHelper.findParent(treeViewOfficeitems, office);
    const isParentRegion = ['Americas', 'Asia/Pacific', 'EMEA', 'Corp/Adj'].some(x => x === parentOffice?.text);
    if (isParentRegion) {
      if (!selectedRegions.some(x => x === parentOffice.text)) {
        selectedRegions.push(parentOffice.text);
      }
    } else {
      this.findParentItem(treeViewOfficeitems, parentOffice, selectedRegions);
    }
  }

  private setVizFilter(filterName, filterValue, filterType, activeSheet) {
    const sheet = activeSheet;
    if (sheet.getSheetType() === 'worksheet') {
      sheet.applyFilterAsync(filterName, filterValue, filterType);
    } else {
      const worksheetArray = sheet.getWorksheets();

      for (let i = 0; i < worksheetArray.length; i++) {
        worksheetArray[i].applyFilterAsync(filterName, filterValue, filterType);
      }

    }
  }

  private getVizFilterValue(filterName, activeSheet) {
    const sheet = activeSheet;
    let filterValue = null;
    const worksheetArray = sheet.getWorksheets();
    for (let i = 0; i < worksheetArray.length; i++) {
      worksheetArray[i].getFilterAsync().then(value => {
        filterValue = value;
      });
    }
    return filterValue;
  }

  private escapeCommaInName(name) {
    if (/,/g.test(name)) {
      name = name.replace(/,/g, '\\,');
    }
    return name;
  }

}
