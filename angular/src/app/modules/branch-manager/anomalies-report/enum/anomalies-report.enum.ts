export enum SelectAllText {
  SELECT_ALL = 'Select All',
  DESELECT = 'Deselect',
  DESELECT_ALL = 'Deselect All'
}

export enum AnomaliesNotes {
  NO_LEAVE_WFH = 'No leave/WFH record',
  NO_EARLY_LEAVE_APPROVAL = 'No early leave/late arrival approval'
}

export enum DataType {
  YESTERDAY = 'yesterday',
  LAST_WEEK = 'lastWeek'
}

export enum TableType {
  ABSENCE = 'absence',
  SHORT = 'short'
}

export enum SortColumn {
  EMPLOYEE_NAME = 'employeeName',
  BRANCH = 'branch',
  ACTUAL_HOURS = 'actualHours',
  COUNT = 'count',
}

export enum SortDirection {
  ASC = 'asc',
  DESC = 'desc',
  NONE = '',
}

export enum SortIcon {
  UNSORTED = 'unfold_more',
  ASCENDING = 'arrow_upward',
  DESCENDING = 'arrow_downward',
}