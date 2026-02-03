import { Component, Input, OnInit, OnChanges, SimpleChanges, Injector } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { AnomaliesReportService, AnomaliesTimelogReportResponse } from '@app/service/api/anomalies-report.service';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';
import { AnomaliesNotes, DataType, SelectAllText, TableType, SortColumn, SortDirection, SortIcon } from './enum/anomalies-report.enum';

@Component({
  selector: 'app-anomalies-report',
  templateUrl: './anomalies-report.component.html',
  styleUrls: ['./anomalies-report.component.css']
})
export class AnomaliesReportComponent extends AppComponentBase implements OnInit {
  selectedTabIndex: number = 0;
  
  @Input() listBranch: BranchDto[] = [];
  @Input() listBranchFilter: BranchDto[] = [];
  
  selectedBranchIds: number[] = [];
  selectedBranchIdsYesterday: number[] = [];
  selectedBranchIdsLastWeek: number[] = [];
  branchSearchText: string = '';
  searchTextYesterday: string = '';
  searchTextLastWeek: string = '';
  
  reportData: AnomaliesTimelogReportResponse | null = null;
  yesterdayReportData: AnomaliesTimelogReportResponse | null = null;
  lastWeekReportData: AnomaliesTimelogReportResponse | null = null;
  
  yesterdayUnplannedAbsences: AnomaliesTimelogReportResponse['yesterdayAnomalies'] = [];
  yesterdayShortWorkingHours: AnomaliesTimelogReportResponse['yesterdayAnomalies'] = [];
  lastWeekUnplannedAbsences: AnomaliesTimelogReportResponse['lastWeekAnomalies'] = [];
  lastWeekShortWorkingHours: AnomaliesTimelogReportResponse['lastWeekAnomalies'] = [];

  filteredYesterdayAbsences: AnomaliesTimelogReportResponse['yesterdayAnomalies'] = [];
  filteredYesterdayShortHours: AnomaliesTimelogReportResponse['yesterdayAnomalies'] = [];
  filteredLastWeekAbsences: AnomaliesTimelogReportResponse['lastWeekAnomalies'] = [];
  filteredLastWeekShortHours: AnomaliesTimelogReportResponse['lastWeekAnomalies'] = [];

  yesterdayAbsenceSort: { column: SortColumn | '', direction: SortDirection } = { column: '', direction: SortDirection.NONE };
  yesterdayShortSort: { column: SortColumn | '', direction: SortDirection } = { column: '', direction: SortDirection.NONE };
  lastWeekAbsenceSort: { column: SortColumn | '', direction: SortDirection } = { column: '', direction: SortDirection.NONE };
  lastWeekShortSort: { column: SortColumn | '', direction: SortDirection } = { column: '', direction: SortDirection.NONE };

  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  
  isLoading: boolean = false;
  
  itemSize: number = 48;
  maxHeight: number = 600;
  Math = Math;

  public DataType = DataType;
  public TableType = TableType;
  
  constructor(
    injector: Injector,
    private anomaliesReportService: AnomaliesReportService
  ) {
    super(injector);
  }
  
  ngOnInit(): void {
    if (this.listBranchFilter && this.listBranchFilter.length > 0) {
      this.selectedBranchIdsYesterday = this.listBranchFilter.map(b => b.id);
      this.selectedBranchIdsLastWeek = this.listBranchFilter.map(b => b.id);
    } else if (this.listBranch && this.listBranch.length > 0) {
      this.selectedBranchIdsYesterday = this.listBranch.map(b => b.id);
      this.selectedBranchIdsLastWeek = this.listBranch.map(b => b.id);
      this.listBranchFilter = [...this.listBranch];
    }
    
    this.selectedBranchIds = [...this.selectedBranchIdsYesterday];
    this.loadYesterdayReport();
    this.loadLastWeekReport();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.listBranch && changes.listBranch.currentValue) {
      this.listBranch = changes.listBranch.currentValue;
      this.listBranchFilter = [...this.listBranch];
    }
  }
  
  filterBranch(searchText?: string): void {
    if (searchText !== undefined) {
      this.branchSearchText = searchText;
    }
    const search = this.branchSearchText.toLowerCase().trim();
    if (!search) {
      this.listBranchFilter = [...this.listBranch];
    } else {
      this.listBranchFilter = this.listBranch.filter(b =>
        b.name.toLowerCase().includes(search)
      );
    }
  }

  get filteredBranches() {
    return this.listBranchFilter.filter(b => b.id && b.id !== 0);
  }

  toggleSelectAll(event: MouseEvent): void {
    event.stopPropagation();
    const selectedIds = this.selectedTabIndex === 0 ? this.selectedBranchIdsYesterday : this.selectedBranchIdsLastWeek;
    
    if (this.isAllSelected() || (selectedIds && selectedIds.length > 0)) {
      if (this.selectedTabIndex === 0) {
        this.selectedBranchIdsYesterday = [];
      } else {
        this.selectedBranchIdsLastWeek = [];
      }
    } else {
      if (this.selectedTabIndex === 0) {
        this.selectedBranchIdsYesterday = this.listBranch.map(b => b.id);
      } else {
        this.selectedBranchIdsLastWeek = this.listBranch.map(b => b.id);
      }
    }
    this.onBranchSelectionChange();
  }

  isAllSelected(): boolean {
    if (this.selectedTabIndex === 0) {
      return this.listBranch.length > 0 &&
             this.selectedBranchIdsYesterday.length === this.listBranch.length;
    } else {
      return this.listBranch.length > 0 &&
             this.selectedBranchIdsLastWeek.length === this.listBranch.length;
    }
  }

  getSelectAllText(): string {
    const selectedIds = this.selectedTabIndex === 0 ? this.selectedBranchIdsYesterday : this.selectedBranchIdsLastWeek;
    
    if (this.isAllSelected()) {
      return SelectAllText.DESELECT_ALL;
    } else if (selectedIds && selectedIds.length > 0) {
      return SelectAllText.DESELECT;
    } else {
      return SelectAllText.SELECT_ALL;
    }
  }

  onTabChange(event: MatTabChangeEvent): void {
    this.selectedTabIndex = event.index;
    if (this.selectedTabIndex === 0) {
      this.selectedBranchIds = [...this.selectedBranchIdsYesterday];
    } else {
      this.selectedBranchIds = [...this.selectedBranchIdsLastWeek];
    }
  }
  
  refresh(): void {
    this.loadYesterdayReport();
    this.loadLastWeekReport();
  }
  
  onBranchSelectionChange(event?: { dataType: DataType; selectedBranchIds: number[] }): void {
    if (event) {
      if (event.dataType === DataType.YESTERDAY) {
        this.selectedBranchIdsYesterday = event.selectedBranchIds;
        this.selectedBranchIds = [...this.selectedBranchIdsYesterday];
        this.loadYesterdayReport();
      } else {
        this.selectedBranchIdsLastWeek = event.selectedBranchIds;
        this.selectedBranchIds = [...this.selectedBranchIdsLastWeek];
        this.loadLastWeekReport();
      }
    } else {
      if (this.selectedTabIndex === 0) {
        this.selectedBranchIds = [...this.selectedBranchIdsYesterday];
        this.loadYesterdayReport();
      } else {
        this.selectedBranchIds = [...this.selectedBranchIdsLastWeek];
        this.loadLastWeekReport();
      }
    }
  }
  
  loadYesterdayReport(): void {
    this.isLoading = true;
    
    this.anomaliesReportService.getAnomaliesTimelogReport(this.selectedBranchIdsYesterday)
      .subscribe({
        next: (data) => {
          this.yesterdayReportData = data;
          this.processYesterdayData(data);
          this.applyYesterdaySearchFilter();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading yesterday report:', error);
          this.isLoading = false;
        }
      });
  }

  loadLastWeekReport(): void {
    this.isLoading = true;
    
    this.anomaliesReportService.getAnomaliesTimelogReport(this.selectedBranchIdsLastWeek)
      .subscribe({
        next: (data) => {
          this.lastWeekReportData = data;
          this.processLastWeekData(data);
          this.applyLastWeekSearchFilter();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading last week report:', error);
          this.isLoading = false;
        }
      });
  }
  
  clearYesterdayData(): void {
    this.yesterdayUnplannedAbsences = [];
    this.yesterdayShortWorkingHours = [];
    this.filteredYesterdayAbsences = [];
    this.filteredYesterdayShortHours = [];
  }

  clearLastWeekData(): void {
    this.lastWeekUnplannedAbsences = [];
    this.lastWeekShortWorkingHours = [];
    this.filteredLastWeekAbsences = [];
    this.filteredLastWeekShortHours = [];
  }

  processYesterdayData(data: AnomaliesTimelogReportResponse): void {
    this.yesterdayUnplannedAbsences = (data.yesterdayAnomalies || [])
      .filter(a => a.notes === AnomaliesNotes.NO_LEAVE_WFH)
      .map(a => ({ ...a, date: this.parseDateFromString(a.date) || a.date }));

    this.yesterdayShortWorkingHours = (data.yesterdayAnomalies || [])
      .filter(a => a.notes === AnomaliesNotes.NO_EARLY_LEAVE_APPROVAL)
      .map(a => ({ ...a, date: this.parseDateFromString(a.date) || a.date }));

    this.filteredYesterdayAbsences = [...this.yesterdayUnplannedAbsences];
    this.filteredYesterdayShortHours = [...this.yesterdayShortWorkingHours];
  }

  processLastWeekData(data: AnomaliesTimelogReportResponse): void {
    this.lastWeekUnplannedAbsences = (data.lastWeekAnomalies || [])
      .filter(a => a.datesMissed && a.datesMissed.length > 0)
      .map(a => ({
        ...a,
        datesMissed: (a.datesMissed || []).map((d: any) => this.parseDateFromString(d) || d),
        count: (a.datesMissed || []).length
      }));

    this.lastWeekShortWorkingHours = (data.lastWeekAnomalies || [])
      .filter(a => (a.datesBelowThreshold && a.datesBelowThreshold.length > 0) || (a.datesNoTrackerTime && a.datesNoTrackerTime.length > 0))
      .map(a => ({
        ...a,
        datesBelowThreshold: (a.datesBelowThreshold || []).map((d: any) => this.parseDateFromString(d) || d),
        datesNoTrackerTime: (a.datesNoTrackerTime || []).map((d: any) => this.parseDateFromString(d) || d),
        count: (a.datesBelowThreshold.length || 0) + (a.datesNoTrackerTime.length || 0)
      }));

    this.filteredLastWeekAbsences = [...this.lastWeekUnplannedAbsences];
    this.filteredLastWeekShortHours = [...this.lastWeekShortWorkingHours];
  }

  private parseDateFromString(dateStr: any): Date | null {
    if (!dateStr) return null;
    if (dateStr instanceof Date) return dateStr;
    if (typeof dateStr !== 'string') return null;
    const parts = dateStr.split('/');
    if (parts.length === 3) {
      const day = ('0' + parts[0]).slice(-2);
      const month = ('0' + parts[1]).slice(-2);
      const year = parts[2];

      const isoFormat = `${year}/${month}/${day}`;
      const date = new Date(isoFormat);
      return isNaN(date.getTime()) ? null : date;
    }
    const parsed = Date.parse(dateStr);
    return isNaN(parsed) ? null : new Date(parsed);
  }
  
  onSearchChange(event: { dataType: DataType; searchText: string }): void {
    if (event.dataType === DataType.YESTERDAY) {
      this.searchTextYesterday = event.searchText;
      this.applyYesterdaySearchFilter();
    } else {
      this.searchTextLastWeek = event.searchText;
      this.applyLastWeekSearchFilter();
    }
  }

  clearSearch(): void {
    if (this.selectedTabIndex === 0) {
      this.searchTextYesterday = '';
      this.applyYesterdaySearchFilter();
    } else {
      this.searchTextLastWeek = '';
      this.applyLastWeekSearchFilter();
    }
  }

  private getFilterFn(search: string): (items: any[]) => any[] {
    const lowerSearch = search.toLowerCase().trim();
    return (items: any[]) => {
      if (!lowerSearch) return [...items];
      return items.filter(item =>
        item.employeeName.toLowerCase().includes(lowerSearch) ||
        item.branch.branchName.toLowerCase().includes(lowerSearch) ||
        item.userName.toLowerCase().includes(lowerSearch)
      );
    };
  }

  applyYesterdaySearchFilter(): void {
    const filterFn = this.getFilterFn(this.searchTextYesterday);
    this.filteredYesterdayAbsences = filterFn(this.yesterdayUnplannedAbsences);
    this.filteredYesterdayShortHours = filterFn(this.yesterdayShortWorkingHours);
  }

  applyLastWeekSearchFilter(): void {
    const filterFn = this.getFilterFn(this.searchTextLastWeek);
    this.filteredLastWeekAbsences = filterFn(this.lastWeekUnplannedAbsences);
    this.filteredLastWeekShortHours = filterFn(this.lastWeekShortWorkingHours);
  }

  sort(column: SortColumn, dataType: DataType, tableType: TableType): void {
    const sortState = this.getSortState(dataType, tableType);
    
    if (sortState.column === column) {
      if (sortState.direction === SortDirection.ASC) {
        sortState.direction = SortDirection.DESC;
      } else if (sortState.direction === SortDirection.DESC) {
        sortState.direction = SortDirection.NONE;
        sortState.column = '';
      }
    } else {
      sortState.column = column;
      sortState.direction = SortDirection.ASC;
    }

    this.applySort(dataType, tableType);
  }

  getSortState(dataType: DataType, tableType: TableType): { column: SortColumn | '', direction: SortDirection } {
    if (dataType === DataType.YESTERDAY) {
      return tableType === TableType.ABSENCE ? this.yesterdayAbsenceSort : this.yesterdayShortSort;
    }
    return tableType === TableType.ABSENCE ? this.lastWeekAbsenceSort : this.lastWeekShortSort;
  }

  applySort(dataType: DataType, tableType: TableType): void {
    const sortState = this.getSortState(dataType, tableType);
    const filtered = this.getFilteredArray(dataType, tableType);
    
    if (!sortState.column || !sortState.direction) {
      const original = this.getDataArray(dataType, tableType);
      this.updateFilteredArray(dataType, tableType, [...original]);
      return;
    }

    const sorted = this.sortData(filtered, sortState.column, sortState.direction);
    this.updateFilteredArray(dataType, tableType, sorted);
  }

  sortData(data: any[], column: SortColumn, direction: SortDirection): any[] {
    if (!direction) return data;

    return [...data].sort((a, b) => {
      let valA: any;
      let valB: any;

      if (column === SortColumn.BRANCH) {
        valA = a.branch.branchName || '';
        valB = b.branch.branchName || '';
      } else if (column === SortColumn.ACTUAL_HOURS || column === SortColumn.COUNT) {
        valA = parseFloat(a[column]) || 0;
        valB = parseFloat(b[column]) || 0;
        return direction === SortDirection.ASC ? valA - valB : valB - valA;
      } else {
        valA = (a[column] !== null && a[column] !== undefined) ? a[column] : '';
        valB = (b[column] !== null && b[column] !== undefined) ? b[column] : '';
      }

      return direction === 'asc' 
        ? valA.toString().localeCompare(valB.toString())
        : valB.toString().localeCompare(valA.toString());
    });
  }

  getDataArray(dataType: string, tableType: string): any[] {
    if (dataType === DataType.YESTERDAY) {
      return tableType === TableType.ABSENCE
        ? this.yesterdayUnplannedAbsences
        : this.yesterdayShortWorkingHours;
    }
    return tableType === TableType.ABSENCE
      ? this.lastWeekUnplannedAbsences
      : this.lastWeekShortWorkingHours;
  }

  getFilteredArray(dataType: string, tableType: string): any[] {
    if (dataType === DataType.YESTERDAY) {
      return tableType === TableType.ABSENCE
        ? this.filteredYesterdayAbsences
        : this.filteredYesterdayShortHours;
    }
    return tableType === TableType.ABSENCE
      ? this.filteredLastWeekAbsences
      : this.filteredLastWeekShortHours;
  }

  updateFilteredArray(dataType: string, tableType: string, data: any[]): void {
    if (dataType === DataType.YESTERDAY) {
      if (tableType === TableType.ABSENCE) this.filteredYesterdayAbsences = data;
      else this.filteredYesterdayShortHours = data;
    } else {
      if (tableType === TableType.ABSENCE) this.filteredLastWeekAbsences = data;
      else this.filteredLastWeekShortHours = data;
    }
  }
  
  getSortIcon(column: SortColumn, dataType: DataType, tableType: TableType): string {
    const sortState = this.getSortState(dataType, tableType);
    if (sortState.column !== column) {
      return SortIcon.UNSORTED;
    }
    return sortState.direction === SortDirection.ASC ? SortIcon.ASCENDING : SortIcon.DESCENDING;
  }
}
export {DataType, TableType};