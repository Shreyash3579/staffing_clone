export interface UserClaim {
    EmployeeCode: string;
    Roles: string[];
    HCPDAccess: HcpdSecurity;
    OfficeAccess: number[];
    FeatureAccess: Feature[];
    DemandTypesAccess: string[];
    PegC2CAccess: boolean;
}

interface Feature {
    FeatureName: string;
    AccessTypeName: string;
}

interface HcpdSecurity {
    EmployeeCode: string;
    SecurityAccessList: HcpdSecurityAccess[]
}

interface HcpdSecurityAccess {
    Office: number;
    PDGradeAccess: string[]
}