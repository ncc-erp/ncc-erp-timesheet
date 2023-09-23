export class AppConsts {

    static remoteServiceBaseUrl: string;
    static appBaseUrl: string;
    static appBaseHref: string; // returns angular's base-href parameter value if used during the publish
    static googleClientAppId: string;//use for google single signon
    static enableNormalLogin: boolean;
    static backendIsNotABP: boolean;
    static urlBeforeLogin:string = ""
    static readonly hrEmailAddress:string = "hr@ncc.asia"


    static localeMappings: any = [];

    static readonly userManagement = {
        defaultAdminUserName: 'admin'
    };

    static readonly localization = {
        defaultLocalizationSourceName: 'Timesheet'
    };

    static readonly authorization = {
        encrptedAuthTokenName: 'enc_auth_token'
    };
}
export const DATE_TIME_OPTIONS = ["Day", "Week", "Month", "Quarter", "Half-Year", "Year", "Custom"];
export const DATE_FILTER_TYPE = ["Onboard", "Be Staff", "Quit Job"];
