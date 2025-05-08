// // import { InMemoryDbService, RequestInfoUtilities, ParsedRequestUrl } from 'angular-in-memory-web-api';

// import { FAKE_APP_SETTINGS, FAKE_EMPLOYEE } from './shared/mocks/mock-core-service';
// import { FAKE_PROJECTS, FAKE_RESOURCEGROUPS, FAKE_OFFICES, FAKE_RESOURCEDETAILS } from './shared/mocks/mock-home-service';

// /** Returns Mocked data that is useful when Real API is not available or under development */
// export class InMemoryDataService implements InMemoryDbService {
//   parseRequestUrl(url: string, utils: RequestInfoUtilities): ParsedRequestUrl  {
//     if (url.toString().indexOf('staffing/api/employee/loggedinuser') !== -1) {
//       url = url.replace(/\/staffing\/api\/employee\/loggedinuser/, '/api/employee');
//     } else if (url.toString().indexOf('staffing/api') !== -1) {
//       url = url.replace(/\/staffing\/api/, '/api');
//     } else if (url.toString().indexOf('/ocelotApiGateway/api/resourceAllocation') !== -1) {
//       url = url.replace(/\/ocelotApiGateway\/api\/resourceAllocation/, '/api/resourceAllocation');
//     } else if (url.toString().indexOf('/ocelotApiGateway/api/project') !== -1) {
//       url = url.replace(/\/ocelotApiGateway\/api\/project\/projectsbyoffices\?startDate=[0-9]{4}-[0-9]{1,2}-[0-9]{1,2}\&endDate=[0-9]{4}-[0-9]{1,2}-[0-9]{1,2}\&officeCodes=.{3}\&offsetStartindex=[0-9]+\&pageSize=[0-9]+/, '/api/projects');
//     } else if (url.toString().indexOf('/ocelotApiGateway/api/office') !== -1) {
//       url = url.replace(/\/ocelotApiGateway\/api\/office\/offices/, '/api/offices');
//     } else if (url.toString().indexOf('/ocelotApiGateway/api/resources') !== -1) {
//       url = url.replace(/\/ocelotApiGateway\/api\/resources\?officeCodes=.{3}/, '/api/resources');
//     } else if (url.toString().indexOf('/ocelotApiGateway/api/aggregations/resourceDetails') !== -1) {
//       url = url.replace(/\/ocelotApiGateway\/api\/aggregations\/resourceDetails\?employeeCode=.{5}/, '/api/resourceDetails');
//     }

//     const parsed = utils.parseRequestUrl(url);
//     return parsed;
//   }

//   createDb() {
//     const employee = FAKE_EMPLOYEE;

//     const appSettings = FAKE_APP_SETTINGS;

//     const projects = {
//       'opportunitiesandallocationsbyoffices': FAKE_PROJECTS.filter(f => f.type === 'Opportunity'),
//       'casesandallocationsbyoffices': FAKE_PROJECTS.filter(f => f.type !== 'Opportunity')
//     };

//     const offices = FAKE_OFFICES;

//     const resources = FAKE_RESOURCEGROUPS;

//     const resourceDetails = FAKE_RESOURCEDETAILS;

//     return {
//       offices: offices,
//       projects: projects,
//       appSettings: appSettings,
//       employee: employee,
//       resources: resources,
//       resourceDetails: resourceDetails
//     };
//   }
// }
