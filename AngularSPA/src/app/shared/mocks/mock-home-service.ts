import { Observable, of } from 'rxjs';
import { Case } from '../interfaces/case.interface';
import { Project } from '../interfaces/project.interface';
import { Office } from '../interfaces/office.interface';
import { ResourceGroup } from '../interfaces/resourceGroup.interface';
import { ResourceAllocation } from '../interfaces/resourceAllocation.interface';
import { Employee } from '../interfaces/employee.interface';
import { Resource } from '../interfaces/resource.interface';
import { CommitmentType } from '../interfaces/commitmentType.interface';
import { InvestmentCategory } from '../interfaces/investmentCateogry.interface';

export const FAKE_PROJECTS_DATA: any = {
  'opportunitiesandallocationsbyoffices': [
    {
      caseName: null,
      oldCaseCode: null,
      caseCode: null,
      clientCode: 9111,
      clientName: 'Apax Partners & Co',
      caseType: null,
      officeAbbreviation: 'NYC',
      officeName: 'New York',
      startDate: '2018-10-30',
      endDate: '2019-01-06',
      duration: '2.25',
      type: 'Opportunity',
      allocatedResources: [
        {
          id: '56431258-adc9-415b-a4c0-60ec18ceac6f',
          clientCode: 0,
          caseCode: null,
          oldCaseCode: 'L7ZJ',
          employeeCode: '12ETH',
          fullName: 'Sheth, Arpan',
          currentLevelGrade: 'V3',
          officeAbbreviation: 'MUB',
          allocation: 20,
          startDate: '0001-01-01T00:00:00',
          endDate: '2019-02-15T00:00:00',
          pipelineId: '7db1fcfe-8b95-4e37-803d-9ccaa2af132c',
          investmentCode: null,
          lastUpdatedBy: null
        },
        {
          id: '2e96e78e-817f-4272-86be-93256bcbab7a',
          clientCode: 0,
          caseCode: null,
          oldCaseCode: 'L7ZJ',
          employeeCode: '41PJH',
          fullName: 'Ghosh, Parijat',
          currentLevelGrade: 'V1',
          officeAbbreviation: 'NDC',
          allocation: 10,
          startDate: '0001-01-01T00:00:00',
          endDate: '2019-02-15T00:00:00',
          pipelineId: '7db1fcfe-8b95-4e37-803d-9ccaa2af132c',
          investmentCode: null,
          lastUpdatedBy: null
        }
      ],
      pipelineId: '7db1fcfe-8b95-4e37-803d-9ccaa2af132c',
      opportunityName: 'Change Management',
      probabilityPercent: 50
    }
  ],
  'casesandallocationsbyoffices': [
    {
      "clientCode": 6455,
      "oldCaseCode": "K6VC",
      "caseCode": 1431,
      "caseName": "Erb Foundation: Phase 2",
      "clientName": "Bridgespan Group",
      "caseType": "Billable",
      "primaryIndustry": null,
      "industryPracticeArea": null,
      "primaryCapability": null,
      "capabilityPracticeArea": null,
      "managingOfficeAbbreviation": "BOS",
      "managingOfficeName": "Boston",
      "billingOfficeAbbreviation": "BOS",
      "billingOfficeName": "Boston",
      "startDate": "2019-06-01T00:00:00",
      "endDate": "2019-08-31T00:00:00",
      "type": "NewDemand",
      "isCaseOnRoll": false,
      "isUpdateEndDateFromCCM": null,
      "caseRollExpectedEndDate": null,
      "allocatedResources": [
        {
          "allocation": 100,
          "caseCode": 291,
          "clientCode": 9111,
          "currentLevelGrade": 'C5',
          "employeeCode": '42ROS',
          "endDate": '2019-03-01T00:00:00',
          "fullName": 'Sapra, Ronika',
          "lastUpdatedBy": null,
          "officeAbbreviation": 'NDC',
          "oldCaseCode": 'N7MM',
          "pipelineId": null,
          "startDate": '2019-01-28T00:00:00'
        }
      ]
    },
    {
      "clientCode": 6455,
      "oldCaseCode": "K5EV",
      "caseCode": 1482,
      "caseName": "NAMI: Revenue and Fundraising RFP In",
      "clientName": "Bridgespan Group",
      "caseType": "Billable",
      "primaryIndustry": null,
      "industryPracticeArea": null,
      "primaryCapability": null,
      "capabilityPracticeArea": null,
      "managingOfficeAbbreviation": "BOS",
      "managingOfficeName": "Boston",
      "billingOfficeAbbreviation": "BOS",
      "billingOfficeName": "Boston",
      "startDate": "2019-06-01T00:00:00",
      "endDate": "2019-09-30T00:00:00",
      "type": "NewDemand",
      "isCaseOnRoll": false,
      "isUpdateEndDateFromCCM": null,
      "caseRollExpectedEndDate": null,
      "allocatedResources": []
    }
  ]
};

export const FAKE_PROJECTS: Project[] = FAKE_PROJECTS_DATA.opportunitiesandallocationsbyoffices.concat(
  FAKE_PROJECTS_DATA.casesandallocationsbyoffices);

export const FAKE_CASES: Case[] = [
  {
    caseName: 'Workers Comp Market Study',
    oldCaseCode: 'T6BW',
    caseCode: 291,
    clientCode: 9111,
    clientName: 'Apax Partners & Co',
    officeAbbreviation: 'NYC',
    startDate: '2018-10-30',
    endDate: '2018-11-19',
    type: 'Opportunity',
    allocatedUsers: []
  },
  {
    caseName: 'Ortho Distribution Expansion',
    oldCaseCode: 'N7MM',
    caseCode: 291,
    clientCode: 9111,
    clientName: 'Stryker',
    officeAbbreviation: 'NYC',
    startDate: '2018-11-05',
    endDate: '2018-11-05',
    type: 'NewDemand',
    allocatedUsers: []
  },
  {
    caseName: 'Project Emppire',
    oldCaseCode: 'L8CN',
    caseCode: 291,
    clientCode: 9111,
    clientName: 'Vista Equity Partners',
    officeAbbreviation: 'NYC',
    startDate: '2018-10-25',
    endDate: '2018-11-14',
    type: 'ActiveCase',
    allocatedUsers: []
  }
];

export const FAKE_RESOURCEGROUPS: any = {
  "allocationsbyoffices": [
    {
      "id": null, "clientCode": 28351, "caseCode": 1, "oldCaseCode": "G3LX", "employeeCode": "36098", "fullName": null, "currentLevelGrade": null, "officeAbbreviation": null, "allocation": 100, "startDate": "2019-04-09T00:00:00", "endDate": "2019-04-19T00:00:00", "pipelineId": null, "investmentCode": null, "lastUpdatedBy": null
    },
    {
      "id": null, "clientCode": 28351, "caseCode": 1, "oldCaseCode": "G3LX", "employeeCode": "36098", "fullName": null, "currentLevelGrade": null, "officeAbbreviation": null, "allocation": 100, "startDate": "2019-02-25T00:00:00", "endDate": "2019-04-08T00:00:00", "pipelineId": null, "investmentCode": null, "lastUpdatedBy": null
    },
    {
      "id": null, "clientCode": 23488, "caseCode": 12, "oldCaseCode": "R6EC", "employeeCode": "36100", "fullName": null, "currentLevelGrade": null, "officeAbbreviation": null, "allocation": 100, "startDate": "2019-02-18T00:00:00", "endDate": "2019-03-31T00:00:00", "pipelineId": null, "investmentCode": null, "lastUpdatedBy": null
    },
    {
      "id": null, "clientCode": 23764, "caseCode": 6, "oldCaseCode": "P3PC", "employeeCode": "36101", "fullName": null, "currentLevelGrade": null, "officeAbbreviation": null, "allocation": 100, "startDate": "2019-02-25T00:00:00", "endDate": "2019-11-30T00:00:00", "pipelineId": null, "investmentCode": null, "lastUpdatedBy": null
    },
    {
      "id": null, "clientCode": 9110, "caseCode": 17654, "oldCaseCode": "K3CV", "employeeCode": "36189", "fullName": null, "currentLevelGrade": null, "officeAbbreviation": null, "allocation": 100, "startDate": "2019-03-18T00:00:00", "endDate": "2019-04-19T00:00:00", "pipelineId": null, "investmentCode": null, "lastUpdatedBy": null
    },
    {
      "id": null, "clientCode": 20386, "caseCode": 94, "oldCaseCode": "G3UM", "employeeCode": "36245", "fullName": null, "currentLevelGrade": null, "officeAbbreviation": null, "allocation": 100, "startDate": "2019-03-20T00:00:00", "endDate": "2019-04-09T00:00:00", "pipelineId": null, "investmentCode": null, "lastUpdatedBy": null
    },
    {
      "id": null, "clientCode": 30300, "caseCode": 2, "oldCaseCode": "P6PS", "employeeCode": "36301", "fullName": null, "currentLevelGrade": null, "officeAbbreviation": null, "allocation": 100, "startDate": "2019-01-02T00:00:00", "endDate": "2019-07-31T00:00:00", "pipelineId": null, "investmentCode": null, "lastUpdatedBy": null
    },
    {
      "id": null, "clientCode": 28351, "caseCode": 1, "oldCaseCode": "G3LX", "employeeCode": "36305", "fullName": null, "currentLevelGrade": null, "officeAbbreviation": null, "allocation": 100, "startDate": "2017-07-31T00:00:00", "endDate": "2019-04-08T00:00:00", "pipelineId": null, "investmentCode": null, "lastUpdatedBy": null
    },
    {
      "id": null, "clientCode": 28351, "caseCode": 1, "oldCaseCode": "G3LX", "employeeCode": "36305", "fullName": null, "currentLevelGrade": null, "officeAbbreviation": null, "allocation": 100, "startDate": "2019-04-09T00:00:00", "endDate": "2019-04-19T00:00:00", "pipelineId": null, "investmentCode": null, "lastUpdatedBy": null
    },
    {
      "id": null, "clientCode": 23764, "caseCode": 6, "oldCaseCode": "P3PC", "employeeCode": "36307", "fullName": null, "currentLevelGrade": null, "officeAbbreviation": null, "allocation": 100, "startDate": "2019-02-25T00:00:00", "endDate": "2019-11-30T00:00:00", "pipelineId": null, "investmentCode": null, "lastUpdatedBy": null
    }
  ],
  "resourcesbyoffices":
    [
      {
        "groupTitle": "Manager", "resources":
          [
            {
              "employeeCode": "38319", "firstName": "Praneet", "lastName": "Gupta", "fullName": "Gupta, Praneet", "levelGrade": "M9", "levelName": "Manager", "startDate": "2016-04-04T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            },
            {
              "employeeCode": "42PTN", "firstName": "Pankaj", "lastName": "Taneja", "fullName": "Taneja, Pankaj", "levelGrade": "M9", "levelName": "Manager", "startDate": "2010-08-16T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            },
            {
              "employeeCode": "42ASD", "firstName": "Arunava", "lastName": "Dalal", "fullName": "Dalal, Arunava", "levelGrade": "M8", "levelName": "Manager", "startDate": "2010-06-14T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            },
            {
              "employeeCode": "45002", "firstName": "Pranjal", "lastName": "Jain", "fullName": "Jain, Pranjal", "levelGrade": "M7", "levelName": "Manager", "startDate": "2018-10-09T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            },
            {
              "employeeCode": "42GAN", "firstName": "Gaurav", "lastName": "Nayyar", "fullName": "Nayyar, Gaurav", "levelGrade": "M7", "levelName": "Manager", "startDate": "2014-08-04T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            }
          ]
      },
      {
        "groupTitle": "Consultant", "resources":
          [
            {
              "employeeCode": "42ISM", "firstName": "Ishita", "lastName": "Mehta", "fullName": "Mehta, Ishita", "levelGrade": "C6", "levelName": "Consultant", "startDate": "2014-07-07T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            },
            {
              "employeeCode": "41SYD", "firstName": "Sangeeta", "lastName": "Yadav", "fullName": "Yadav, Sangeeta", "levelGrade": "C6", "levelName": "Consultant", "startDate": "2014-06-19T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            },
            {
              "employeeCode": "36301", "firstName": "Avi", "lastName": "Katyal", "fullName": "Katyal, Avi", "levelGrade": "C5", "levelName": "Consultant", "startDate": "2015-07-06T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 0.4
            },
            {
              "employeeCode": "36808", "firstName": "Piyush", "lastName": "Pathneja", "fullName": "Pathneja, Piyush", "levelGrade": "C5", "levelName": "Consultant", "startDate": "2015-08-03T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 0.2
            },
            {
              "employeeCode": "42ROS", "firstName": "Ronika", "lastName": "Sapra", "fullName": "Sapra, Ronika", "levelGrade": "C5", "levelName": "Consultant", "startDate": "2018-09-03T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            }
          ]
      },
      {
        "groupTitle": "Associate Consultant", "resources":
          [
            {
              "employeeCode": "38035", "firstName": "Anand", "lastName": "Bhusry", "fullName": "Bhusry, Anand", "levelGrade": "A6", "levelName": "Associate Consultant", "startDate": "2017-06-05T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            },
            {
              "employeeCode": "38791", "firstName": "Meghank", "lastName": "Garg", "fullName": "Garg, Meghank", "levelGrade": "A6", "levelName": "Associate Consultant", "startDate": "2016-08-01T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            },
            {
              "employeeCode": "41107", "firstName": "Sagnik", "lastName": "Ghosh", "fullName": "Ghosh, Sagnik", "levelGrade": "A6", "levelName": "Associate Consultant", "startDate": "2017-07-03T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            },
            {
              "employeeCode": "38036", "firstName": "Dhruv", "lastName": "Gulati", "fullName": "Gulati, Dhruv", "levelGrade": "A6", "levelName": "Associate Consultant", "startDate": "2017-06-05T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            },
            {
              "employeeCode": "42DEG", "firstName": "Devyani", "lastName": "Gupta", "fullName": "Gupta, Devyani", "levelGrade": "A6", "levelName": "Associate Consultant", "startDate": "2014-07-23T00:00:00", "terminationDate": null, "office": { "officeCode": 404, "officeName": "New Delhi", "officeAbbreviation": "NDC" }, "fte": 1
            }
          ]
      }
    ]
};

//export const FAKE_RESOURCEGROUPS: ResourceGroup[] = [
//  {
//    'groupTitle': 'Manager',
//    'resources': [
//      {
//        "employeeCode": "01RRU",
//        "firstName": "Rob",
//        "lastName": "Ruffin",
//        "fullName": "Ruffin, Rob",
//        "levelGrade": "M9",
//        "levelName": "Manager",
//        "dateFirstAvailable": "10-Jun-2019",
//        "percentAvailable": 50,
//        "office": {
//          "officeCode": 110,
//          "officeName": "Boston",
//          "officeAbbreviation": "BOS"
//        }
//      },
//      {
//        "employeeCode": "44816",
//        "firstName": "Mark",
//        "lastName": "Burton",
//        "fullName": "Burton, Mark",
//        "levelGrade": "M9",
//        "levelName": "Manager",
//        "dateFirstAvailable": "10-Jun-2019",
//        "percentAvailable": 50,
//        "office": {
//          "officeCode": 110,
//          "officeName": "Boston",
//          "officeAbbreviation": "BOS"
//        }
//      },
//      {
//        "employeeCode": "43205",
//        "firstName": "Bala",
//        "lastName": "Parameshwaran",
//        "fullName": "Parameshwaran, Bala",
//        "levelGrade": "M7",
//        "levelName": "Manager",
//        "dateFirstAvailable": "10-Jun-2019",
//        "percentAvailable": 50,
//        "office": {
//          "officeCode": 110,
//          "officeName": "Boston",
//          "officeAbbreviation": "BOS"
//        }
//      }
//    ]
//  },
//  {
//    "groupTitle": "Consultant",
//    "resources": [
//      {
//        "employeeCode": "01JZK",
//        "firstName": "Joseph",
//        "lastName": "Kiernan",
//        "fullName": "Kiernan, Joseph",
//        "levelGrade": "C5",
//        "levelName": "Consultant",
//        "dateFirstAvailable": "10-Jun-2019",
//        "percentAvailable": 50,
//        "office": {
//          "officeCode": 110,
//          "officeName": "Boston",
//          "officeAbbreviation": "BOS"
//        }
//      },
//      {
//        "employeeCode": "38218",
//        "firstName": "Glory",
//        "lastName": "Nguyen",
//        "fullName": "Nguyen, Glory",
//        "levelGrade": "C5",
//        "levelName": "Consultant",
//        "dateFirstAvailable": "10-Jun-2019",
//        "percentAvailable": 50,
//        "office": {
//          "officeCode": 110,
//          "officeName": "Boston",
//          "officeAbbreviation": "BOS"
//        }
//      },
//      {
//        "employeeCode": "38462",
//        "firstName": "Ned",
//        "lastName": "Sebelius",
//        "fullName": "Sebelius, Ned",
//        "levelGrade": "C5",
//        "levelName": "Consultant",
//        "dateFirstAvailable": "10-Jun-2019",
//        "percentAvailable": 50,
//        "office": {
//          "officeCode": 110,
//          "officeName": "Boston",
//          "officeAbbreviation": "BOS"
//        }
//      }
//    ]
//  },
//  {
//    "groupTitle": "Associate Consultant",
//    "resources": [
//      {
//        "employeeCode": "37569",
//        "firstName": "Enping",
//        "lastName": "Song",
//        "fullName": "Song, Enping",
//        "levelGrade": "A6",
//        "levelName": "Associate Consultant",
//        "dateFirstAvailable": "10-Jun-2019",
//        "percentAvailable": 50,
//        "office": {
//          "officeCode": 110,
//          "officeName": "Boston",
//          "officeAbbreviation": "BOS"
//        }
//      },
//      {
//        "employeeCode": "01APX",
//        "firstName": "Ali",
//        "lastName": "Piltch",
//        "fullName": "Piltch, Ali",
//        "levelGrade": "A6",
//        "levelName": "Associate Consultant",
//        "dateFirstAvailable": "10-Jun-2019",
//        "percentAvailable": 50,
//        "office": {
//          "officeCode": 110,
//          "officeName": "Boston",
//          "officeAbbreviation": "BOS"
//        }
//      },
//      {
//        "employeeCode": "01ABX",
//        "firstName": "Andrew",
//        "lastName": "Bravo",
//        "fullName": "Bravo, Andrew",
//        "levelGrade": "A6",
//        "levelName": "Associate Consultant",
//        "dateFirstAvailable": "10-Jun-2019",
//        "percentAvailable": 50,
//        "office": {
//          "officeCode": 110,
//          "officeName": "Boston",
//          "officeAbbreviation": "BOS"
//        }
//      }
//    ]
//  }
//];

export const FAKE_OFFICES: Office[] = [
  {
    'officeCode': 115,
    'officeName': 'Atlanta',
    'officeAbbreviation': 'ATL'
  },
  {
    'officeCode': 110,
    'officeName': 'Boston',
    'officeAbbreviation': 'BOS'
  },
  {
    'officeCode': 160,
    'officeName': 'Chicago',
    'officeAbbreviation': 'CHI'
  },
  {
    'officeCode': 150,
    'officeName': 'Dallas',
    'officeAbbreviation': 'DAL'
  },
  {
    'officeCode': 210,
    'officeName': 'London',
    'officeAbbreviation': 'LON'
  },
  {
    'officeCode': 505,
    'officeName': 'Milan',
    'officeAbbreviation': 'MLN'
  },
  {
    'officeCode': 404,
    'officeName': 'New Delhi',
    'officeAbbreviation': 'NDC'
  },
  {
    'officeCode': 112,
    'officeName': 'New York',
    'officeAbbreviation': 'NYC'
  },
  {
    'officeCode': 230,
    'officeName': 'Paris',
    'officeAbbreviation': 'PAR'
  },
  {
    'officeCode': 323,
    'officeName': 'Singapore',
    'officeAbbreviation': 'SIN'
  },
  {
    'officeCode': 320,
    'officeName': 'Sydney',
    'officeAbbreviation': 'SYD'
  }
];
export const FAKE_RESOURCEDETAILS: any =
  [
    {
      'staffinghistory':
        [
          {
            'caseCode': 1,
            'oldCaseCode': 'T7GQ',
            'caseName': 'Due Diligence of Downstream Asset',
            'clientName': 'Arcelor Mittal',
            'levelGrade': 'M9',
            'allocation': '100%',
            'startDate': '2019-02-05T00:00:00',
            'endDate': '2019-02-22T00:00:00',
            'primaryIndustry': 'Steel',
            'primaryCapability': 'Commercial Due Diligence - Corp M&A'
          },
          {
            'caseCode': 1,
            'oldCaseCode': 'T7GQ',
            'caseName': 'Due Diligence of Downstream Asset',
            'clientName': 'Arcelor Mittal',
            'levelGrade': 'M9',
            'allocation': '100%',
            'startDate': '2019-01-07T00:00:00',
            'endDate': '2019-02-04T00:00:00',
            'primaryIndustry': 'Steel',
            'primaryCapability': 'Commercial Due Diligence - Corp M&A'
          },
          {
            'caseCode': 18390,
            'oldCaseCode': 'W4BZ',
            'caseName': 'Arcelor Mittal',
            'clientName': 'Non-Billable',
            'levelGrade': 'M9',
            'allocation': '100%',
            'startDate': '2019-01-02T00:00:00',
            'endDate': '2019-01-04T00:00:00',
            'primaryIndustry': null,
            'primaryCapability': null
          }
        ],
      'vacationrequests':
        [
          {
            'startDate': '2019-03-07T00:00:00',
            'endDate': '2019-03-09T00:00:00',
            'description': '2 day personal leave',
            'status': 'Pending',
            'type': 'Vacation'
          },
          {
            'startDate': '2019-12-26T00:00:00',
            'endDate': '2019-12-31T00:00:00',
            'description': '4 day personal leave',
            'status': 'Pending',
            'type': 'Vacation'
          }
        ]
    },
    {
      'employeeCode': '38319',
      'firstName': 'Praneet',
      'lastName': 'Gupta',
      'fullName': 'Praneet Gupta',
      'levelName': 'Principal',
      'levelGrade': 'M9',
      'internetAddress': 'praneet.gupta@bain.com',
      'position': null,
      'serviceLine': null,
      'office':
      {
        'officeCode': 404,
        'officeName': 'New Delhi',
        'officeAbbreviation': 'NDC'
      },
      'profileImageUrl':
        'data:image/jpg;base64,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',
      'token': null,
      "fte": 1
    }
  ];

export const FAKE_SEARCHED_RESOURCES: Resource[] = [
  {
    "employeeCode": "01EMS",
    "firstName": "Erika",
    "lastName": "Serow",
    "fullName": "Serow, Erika",
    "levelGrade": "V3",
    "levelName": "Partner",
    "office": {
      "officeCode": 112,
      "officeName": "New York",
      "officeAbbreviation": "NYC"
    },
    "schedulingOffice":null,
    "dateFirstAvailable": "10-Jun-2019",
    "percentAvailable": 50,
    "serviceLine":null,
    "fte": 1,
    "position":null,
    "upcomingCommitmentsForAlerts" : [],
    "availabilityStatus":"Available",
    "onTransitionOrTerminationAndNotAvailable": false
  }//,
  // {
  //   "employeeCode": "42161",
  //   "firstName": "Erika",
  //   "lastName": "Bartucca",
  //   "fullName": "Bartucca, Erika",
  //   "levelGrade": "N1",
  //   "levelName": "Administrative",
  //   "dateFirstAvailable": "10-Jun-2019",
  //   "percentAvailable": 50,
  //   "office": {
  //     "officeCode": 110,
  //     "officeName": "Boston",
  //     "officeAbbreviation": "BOS"
  //   },
  //   "schedulingOffice":null, "fte": 1,
  //   "serviceLine":null,
  //   "position":null,
  //   "upcomingCommitmentsForAlerts" : [],
  //   "availabilityStatus":"Available"
  // },
  // {
  //   "employeeCode": "02EGO",
  //   "firstName": "Erika",
  //   "lastName": "Gottschalk",
  //   "fullName": "Gottschalk, Erika",
  //   "levelGrade": "N1",
  //   "levelName": "Administrative",
  //   "dateFirstAvailable": "10-Jun-2019",
  //   "percentAvailable": 50,
  //   "office": {
  //     "officeCode": 125,
  //     "officeName": "San Francisco",
  //     "officeAbbreviation": "SFR"
  //   },
  //   "schedulingOffice":null, "fte": 1,
  //   "serviceLine":null,
  //   "position":null,
  //   "upcomingCommitmentsForAlerts" : [],
  //   "availabilityStatus":"Available"
  // },
  // {
  //   "employeeCode": "01HUG",
  //   "firstName": "Erika",
  //   "lastName": "Schneider",
  //   "fullName": "Schneider, Erika",
  //   "levelGrade": "N1",
  //   "levelName": "Administrative",
  //   "dateFirstAvailable": "10-Jun-2019",
  //   "percentAvailable": 50,
  //   "office": {
  //     "officeCode": 110,
  //     "officeName": "Boston",
  //     "officeAbbreviation": "BOS"
  //   },
  //   "schedulingOffice":null, "fte": 1,
  //   "serviceLine":null,
  //   "position":null,
  //   "upcomingCommitmentsForAlerts" : [],
  //   "availabilityStatus":"Available"
  // },
  // {
  //   "employeeCode": "37316",
  //   "firstName": "Erika",
  //   "lastName": "Tannaka",
  //   "fullName": "Tannaka, Erika",
  //   "levelGrade": "N1",
  //   "levelName": "Administrative",
  //   "dateFirstAvailable": "10-Jun-2019",
  //   "percentAvailable": 50,
  //   "office": {
  //     "officeCode": 310,
  //     "officeName": "Tokyo",
  //     "officeAbbreviation": "TOK"
  //   },
  //   "schedulingOffice":null, "fte": 1,
  //   "serviceLine":null,
  //   "position":null,
  //   "upcomingCommitmentsForAlerts" : [],
  //   "availabilityStatus":"Available"
  // },
  // {
  //   "employeeCode": "42219",
  //   "firstName": "Erika",
  //   "lastName": "Wuerzner",
  //   "fullName": "Wuerzner, Erika",
  //   "levelGrade": "N1",
  //   "levelName": "Administrative",
  //   "dateFirstAvailable": "10-Jun-2019",
  //   "percentAvailable": 50,
  //   "office": {
  //     "officeCode": 115,
  //     "officeName": "Atlanta",
  //     "officeAbbreviation": "ATL"
  //   },
  //   "schedulingOffice":null, "fte": 1,
  //   "serviceLine":null,
  //   "position":null,
  //   "upcomingCommitmentsForAlerts" : [],
  //   "availabilityStatus":"Available"
  // },
  // {
  //   "employeeCode": "36634",
  //   "firstName": "Erika",
  //   "lastName": "Kain",
  //   "fullName": "Kain, Erika",
  //   "levelGrade": "C5",
  //   "levelName": "Consultant",
  //   "dateFirstAvailable": "10-Jun-2019",
  //   "percentAvailable": 50,
  //   "office": {
  //     "officeCode": 116,
  //     "officeName": "Washington, D.C.",
  //     "officeAbbreviation": "WAS"
  //   },
  //   "schedulingOffice":null, "fte": 1,
  //   "serviceLine":null,
  //   "position":null,
  //   "upcomingCommitmentsForAlerts" : [],
  //   "availabilityStatus":"Available"
  // },
  // {
  //   "employeeCode": "38401",
  //   "firstName": "Erika",
  //   "lastName": "Hines",
  //   "fullName": "Hines, Erika",
  //   "levelGrade": "C3",
  //   "levelName": "Consultant",
  //   "dateFirstAvailable": "10-Jun-2019",
  //   "percentAvailable": 50,
  //   "office": {
  //     "officeCode": 115,
  //     "officeName": "Atlanta",
  //     "officeAbbreviation": "ATL"
  //   },
  //   "schedulingOffice":null, "fte": 1,
  //   "serviceLine":null,
  //   "position":null,
  //   "upcomingCommitmentsForAlerts" : [],
  //   "availabilityStatus":"Available"
  // },
  // {
  //   "employeeCode": "36721",
  //   "firstName": "Erika",
  //   "lastName": "Tsang",
  //   "fullName": "Tsang, Erika",
  //   "levelGrade": "A5",
  //   "levelName": "Associate Consultant",
  //   "dateFirstAvailable": "10-Jun-2019",
  //   "percentAvailable": 50,
  //   "office": {
  //     "officeCode": 120,
  //     "officeName": "Toronto",
  //     "officeAbbreviation": "TOR"
  //   },
  //   "schedulingOffice":null, "fte": 1,
  //   "serviceLine":null,
  //   "position":null,
  //   "upcomingCommitmentsForAlerts" : [],
  //   "availabilityStatus":"Available"
  // },
  // {
  //   "employeeCode": "46182",
  //   "firstName": "Erika",
  //   "lastName": "Kurachi",
  //   "fullName": "Kurachi, Erika",
  //   "levelGrade": "A1",
  //   "levelName": "Associate Consultant",
  //   "dateFirstAvailable": "10-Jun-2019",
  //   "percentAvailable": 50,
  //   "office": {
  //     "officeCode": 310,
  //     "officeName": "Tokyo",
  //     "officeAbbreviation": "TOK"
  //   },
  //   "schedulingOffice":null,
  //    "fte": 1,
  //   "serviceLine":null,
  //   "position":null,
  //   "upcomingCommitmentsForAlerts" : [],
  //   "availabilityStatus":"Available"
  // }
]

export const FAKE_COMMITMEMT_TYPES: CommitmentType[] = [
  {
    "commitmentTypeCode": "C", "commitmentTypeName": "Assignment", "precedence": 1
  },
  {
    "commitmentTypeCode": "L", "commitmentTypeName": "Leave of absence", "precedence": 2
  },
  {
    "commitmentTypeCode": "P", "commitmentTypeName": "PEG", "precedence": 3
  },
  {
    "commitmentTypeCode": "R", "commitmentTypeName": "Recruting", "precedence": 4
  },
  {
    "commitmentTypeCode": "T", "commitmentTypeName": "Training", "precedence": 5
  },
  {
    "commitmentTypeCode": "V", "commitmentTypeName": "Vacation", "precedence": 6
  }
]

export const FAKE_INVESTMENT_CATEGORIES: InvestmentCategory[] = [
  {
    "investmentCode": 12, "investmentName": "Backfill", "investmentDescription": "Backfill", "precedence": 1
  },
  {
    "investmentCode": 1, "investmentName": "Client Development", "investmentDescription": "Work tied to a target client and specific potential piece of work (can be related to a practice via the CD case code) ", "precedence": 2
  },
  {
    "investmentCode": 2, "investmentName": "Client Variable", "investmentDescription": "Can be changed by staffing and tied to a particular individual, i.e., additional resources staffed at case partner's request to get the job done", "precedence": 3
  },
  {
    "investmentCode": 7, "investmentName": "Internal local office role", "investmentDescription": "Internal local office roles", "precedence": 4
  },
  {
    "investmentCode": 9, "investmentName": "Internal other", "investmentDescription": "Internal other", "precedence": 5
  },
  {
    "investmentCode": 8, "investmentName": "Internal pro bono", "investmentDescription": "Internal pro bono", "precedence": 6
  },
  {
    "investmentCode": 4, "investmentName": "Pre/Post Revenue", "investmentDescription": "Specific individuals held before, in between, or after paid workstreams to secure future revenue (allocation extends outside of BASIS case dates)", "precedence": 7
  }
]

export const FAKE_OPPORTUNITY_AUDIT_DATA: any = [
  {
    "eventDescription": "EndDate for Gupta, Praneet is updated",
    "employee": "38319",
    "old": "22-Nov-2019",
    "new": "20-Nov-2019",
    "updatedBy": "Agarwal, Anshul",
    "date": "2019-05-15T09:58:31.1566667Z"
  },
  {
    "eventDescription": "Gupta, Praneet is assigned",
    "employee": "38319",
    "old": null,
    "new": null,
    "updatedBy": "Upadhyaya, Dheeraj",
    "date": "2019-05-15T09:26:02.1466667Z"
  },
  {
    "eventDescription": "Upadhyaya, Dheeraj is removed",
    "employee": "45088",
    "old": null,
    "new": null,
    "updatedBy": "Upadhyaya, Dheeraj",
    "date": "2019-05-15T09:13:51.87Z"
  },
  {
    "eventDescription": "Upadhyaya, Dheeraj is assigned",
    "employee": "45088",
    "old": null,
    "new": null,
    "updatedBy": "Upadhyaya, Dheeraj",
    "date": "2019-05-15T09:13:51.87Z"
  },
  {
    "eventDescription": "Abate, Brittany is removed",
    "employee": "44751",
    "old": null,
    "new": null,
    "updatedBy": "Upadhyaya, Dheeraj",
    "date": "2019-05-15T09:13:37.4266667Z"
  },
  {
    "eventDescription": "Abate, Brittany is assigned",
    "employee": "44751",
    "old": null,
    "new": null,
    "updatedBy": "Upadhyaya, Dheeraj",
    "date": "2019-05-15T09:13:37.4266667Z"
  },
  {
    "eventDescription": "Abate, Brittany is removed",
    "employee": "44751",
    "old": null,
    "new": null,
    "updatedBy": "Upadhyaya, Dheeraj",
    "date": "2019-05-15T08:42:38.7766667Z"
  },
  {
    "eventDescription": "Abate, Brittany is assigned",
    "employee": "44751",
    "old": null,
    "new": null,
    "updatedBy": "Upadhyaya, Dheeraj",
    "date": "2019-05-15T08:42:38.7766667Z"
  },
  {
    "eventDescription": "Chandra, Gobin is removed",
    "employee": "22GOC",
    "old": null,
    "new": null,
    "updatedBy": "Upadhyaya, Dheeraj",
    "date": "2019-05-15T07:39:24.31Z"
  },
  {
    "eventDescription": "Chandra, Gobin is assigned",
    "employee": "22GOC",
    "old": null,
    "new": null,
    "updatedBy": "Upadhyaya, Dheeraj",
    "date": "2019-05-15T07:39:24.31Z"
  },
  {
    "eventDescription": "Nayyar, Gaurav is removed",
    "employee": "42GAN",
    "old": null,
    "new": null,
    "updatedBy": "Agarwal, Anshul",
    "date": "2019-05-14T18:38:07.3833333Z"
  },
  {
    "eventDescription": "Nayyar, Gaurav is assigned",
    "employee": "42GAN",
    "old": null,
    "new": null,
    "updatedBy": "Agarwal, Anshul",
    "date": "2019-05-14T18:38:07.3833333Z"
  },
  {
    "eventDescription": "Upmanyu, Arjun is removed",
    "employee": "42AUM",
    "old": null,
    "new": null,
    "updatedBy": "Agarwal, Anshul",
    "date": "2019-05-14T18:36:13.7666667Z"
  },
  {
    "eventDescription": "Upmanyu, Arjun is assigned",
    "employee": "42AUM",
    "old": null,
    "new": null,
    "updatedBy": "Agarwal, Anshul",
    "date": "2019-05-14T18:36:13.7666667Z"
  },
  {
    "eventDescription": "Nayyar, Gaurav is removed",
    "employee": "42GAN",
    "old": null,
    "new": null,
    "updatedBy": "Agarwal, Anshul",
    "date": "2019-05-14T18:34:36.9666667Z"
  },
  {
    "eventDescription": "Nayyar, Gaurav is assigned",
    "employee": "42GAN",
    "old": null,
    "new": null,
    "updatedBy": "Agarwal, Anshul",
    "date": "2019-05-14T18:34:36.9666667Z"
  }
]

export class MockHomeService {
  public getProjectsFilteredBySelectedValues(startDate, endDate, officeCode): Observable<Project[]> {
    return of(FAKE_PROJECTS_DATA);
  }

  public getActiveCases(startDate, endDate, officeCode): Observable<Case[]> {
    return of(FAKE_CASES);
  }

  public getActiveCasesByMultipleOffices(startDate, endDate, officeCodes): Observable<Case[]> {
    return of(FAKE_CASES);
  }

  public getOfficeList(): Observable<Office[]> {
    return of(FAKE_OFFICES);
  }

  //public getResourcesByOffices(): Observable<ResourceGroup[]> {
  //  return of(FAKE_RESOURCEGROUPS);
  //}

  public getAvailableResourcesByOffices(): Observable<ResourceGroup[]> {
    return of(FAKE_RESOURCEGROUPS);
  }

  public getResourcesBySearchString(): Observable<Resource[]> {
    return of(FAKE_SEARCHED_RESOURCES);
  }

  public getResourceDetails(): Observable<any> {
    return of(FAKE_RESOURCEDETAILS);
  }

  public mapResourceToProject(resourceAllocation: ResourceAllocation): Observable<ResourceAllocation> {
    let cases = [];
    let projects = [];
    projects = FAKE_PROJECTS_DATA.opportunitiesandallocationsbyoffices;
    cases = FAKE_PROJECTS_DATA.casesandallocationsbyoffices;
    projects = projects.concat(cases);

    const assignedProjectIndex = projects.findIndex(p => p.oldCaseCode === resourceAllocation.oldCaseCode);

    const allocatedResourceIndex = projects[assignedProjectIndex].allocatedResources
      .findIndex(e => e.employeeCode === resourceAllocation.employeeCode);
    if (!Array.isArray(projects[assignedProjectIndex].allocatedResources)) {
      projects[assignedProjectIndex].allocatedResources = [];
    }
    projects[assignedProjectIndex].allocatedResources.splice(allocatedResourceIndex, 0, resourceAllocation);
    resourceAllocation['id'] = '6ef70932-123f-e911-a990-005056ac2b1d';
    return of(resourceAllocation);
  }

  public getCommitmentTypes(): Observable<CommitmentType[]> {
    return of(FAKE_COMMITMEMT_TYPES);
  }

  public getInvestmentCategories(): Observable<InvestmentCategory[]> {
    return of(FAKE_INVESTMENT_CATEGORIES);
  }

  getOpportunityAndAllocationsByPipelineId(pipelineId): Observable<any> {
    return of(FAKE_PROJECTS_DATA.opportunitiesandallocationsbyoffices.concat(FAKE_OPPORTUNITY_AUDIT_DATA));
  }

}
