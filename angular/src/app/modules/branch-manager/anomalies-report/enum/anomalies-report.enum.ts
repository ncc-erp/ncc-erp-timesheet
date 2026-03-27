export enum SelectAllText {
  SELECT_ALL = 'Select All',
  DESELECT = 'Deselect',
  DESELECT_ALL = 'Deselect All'
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
  UNSORTED = 'pi-sort-alt',
  ASCENDING = 'pi-sort-amount-up',
  DESCENDING = 'pi-sort-amount-down',
}