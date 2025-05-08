import { Observable, of} from 'rxjs';
import { Employee } from '../interfaces/employee.interface'
import { AppSettings } from '../interfaces/appSettings.interface'
import { UserNotification } from '../interfaces/userNotification.interface';

export const FAKE_EMPLOYEE: Employee = {
  employeeCode: "37995",
  firstName: "Nitin",
  lastName: "Jain",
  fullName: "Nitin Jain",
  levelName: "Administrative",
  levelGrade: "N1",
  internetAddress: "nitin.jain@bain.com",
  profileImageUrl: null,
  startDate: null,
  activeStatus: 'active',
  terminationDate: null,
  office: {
    officeCode: 332,
    officeName: "New Delhi - BCC",
    officeAbbreviation : "NDS"
  },
  schedulingOffice: {
    officeCode: 332,
    officeName: "New Delhi - BCC",
    officeAbbreviation : "NDS"
  },
  serviceLine: null,
  position: null,
  fte: 1,
  isAdmin:false,
  isTerminated: false,
  override: false,
  mentorEcode: null,
  mentorName: null,
  token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjQ1MDg4IiwibmJmIjoxNTQ3MDMwMTUzLCJleHAiOjE1NDc2MzQ5NTMsImlhdCI6MTU0NzAzMDE1M30.5mJSRXwo5h-59XR3WEIM9WPfwH25yIE0myFUvRHfhqI'
}

export const FAKE_APP_SETTINGS: AppSettings = {
  ocelotApiGatewayBaseUrl: 'http://wtrstfwebdev1v:82/ocelotApiGateway',
  staffingAuthenticationApiBaseUrl : '',
  projectsPageSize: 20,
  resourcesPerPage: 20,
  scrollDistance: 3,
  googleAnalyticsJqueryUrl: '',
  googleAnalyticsJsUrl: '',
  googleAnalyticsProfileUrl: '',
  clearLocalStorage: 'false',
  siteUnderMaintainance: 'false'
}

export const FAKE_USER_NOTIFICATIONS: UserNotification[] = [
  {
    notificationId: "eae23d64-8c99-e911-a992-005056ac4aea",
    description: "Case Ending - Case Project Tyson - D7VQ is ending",
    notificationDate: new Date("2019-06-28T06:06:27.76"),
    notificationDueDate: new Date("2019-07-09T00:00:00"),
    notificationStatus: "U"
  },
  {
    notificationId: "d4e23d64-8c99-e911-a992-005056ac4aea",
    description: "Case Ending - Case Project Cosmos - S2CT is ending",
    notificationDate: new Date("2019-06-28T06:06:27.76"),
    notificationDueDate: new Date("2019-07-10T00:00:00"),
    notificationStatus: "U"
  },
  {
    notificationId: "1be33d64-8c99-e911-a992-005056ac4aea",
    description: "Case Ending - Case SG&A Cost Transformation - Q3UA is ending",
    notificationDate: new Date("2019-06-28T06:06:27.76"),
    notificationDueDate: new Date("2019-07-10T00:00:00"),
    notificationStatus: "U"
  },
  {
    notificationId: "27e33d64-8c99-e911-a992-005056ac4aea",
    description: "Case Ending - Case GBS-Integrated Services Redesign - V7LE is ending",
    notificationDate: new Date("2019-06-28T06:06:27.76"),
    notificationDueDate: new Date("2019-07-12T00:00:00"),
    notificationStatus: "U"
  },
  {
    notificationId: "408fff98-c09b-e911-a992-005056ac4aea",
    description: "Case Ending - Case Project Quantum - K7RQ is ending",
    notificationDate: new Date("2019-07-01T01:25:12.3"),
    notificationDueDate: new Date("2019-07-15T00:00:00"),
    notificationStatus: "U"
  },
  {
    notificationId: "468fff98-c09b-e911-a992-005056ac4aea",
    description: "Case Ending - Case Telstra - CTSU - U7KZ is ending",
    notificationDate: new Date("2019-07-01T01:25:12.3"),
    notificationDueDate: new Date("2019-07-15T00:00:00"),
    notificationStatus: "U"
  }
]

export class MockCoreService {
  public loggedInUser: Employee = FAKE_EMPLOYEE;
  public appSettings: AppSettings = FAKE_APP_SETTINGS;

  public getAppSettings(): Observable<AppSettings> {
    return of(FAKE_APP_SETTINGS);
  }

  public getEmployee(): Observable<Employee> {
    return of(FAKE_EMPLOYEE);
  }

  public getUserNotifications(): Observable<UserNotification[]>{
    return of(FAKE_USER_NOTIFICATIONS);
  }

}
