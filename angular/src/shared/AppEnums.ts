import { IsTenantAvailableOutputState } from "@shared/service-proxies/service-proxies";

export class AppTenantAvailabilityState {
  static Available: number = IsTenantAvailableOutputState._1;
  static InActive: number = IsTenantAvailableOutputState._2;
  static NotFound: number = IsTenantAvailableOutputState._3;
}
export enum DateSelectorEnum {
  DAY = "Day",
  WEEK = "Week",
  MONTH = "Month",
  QUARTER = "Quarter",
  HALF_YEAR = "Half-Year",
  YEAR = "Year",
  CUSTOM = "Custom",
}
export enum ExpressionEnum {
  NO_FILTER = 0,
  LESS_OR_EQUAL = 1,
  LARGER_OR_EQUAL = 2,
  EQUAL = 3,
  FT = 4,
}
export enum BTransactionStatusColor {
  PENDING = "#E79C07",
  DONE = "#07E714",
  DEFAULT = "",
}
export enum DateFilterType {
  OnBoardDate,
  BeStaffDate,
}

export enum ActionDialog {
  CREATE,
  EDIT,
}
export enum CapabilityType {
  Point = 0,
  Text = 1,
}
export enum EComparisionOperator {
  Equal,
  LessThan,
  LessThanOrEqual,
  GreaterThan,
  GreaterThanOrEqual,
  NotEqual,
  Contains, //for strings  
  StartsWith, //for strings  
  EndsWith, //for strings  
  In // for list item
}
export enum CapabilityUserType {
  Staff = 0,
  Internship = 1,
  Collaborators = 2,
  ProbationaryStaff = 3,
  Vendor = 5
}
export enum SortDirectionEnum {
  Ascending = 0,
  Descending = 1
}

