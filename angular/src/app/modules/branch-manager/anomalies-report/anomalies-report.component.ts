import { Component, Input, OnInit, OnChanges, SimpleChanges, ViewChild, Injector } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { AnomaliesReportService, AnomaliesTimelogReportResponse } from '@app/service/api/anomalies-report.service';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/app-component-base';

type SortColumn = 'employeeName' | 'branch' | 'actualHours' | 'count';
type SortDirection = 'asc' | 'desc' | '';

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
  searchText: string = '';
  
  reportData: AnomaliesTimelogReportResponse | null = null;
  yesterdayReportData: AnomaliesTimelogReportResponse | null = null;
  lastWeekReportData: AnomaliesTimelogReportResponse | null = null;
  
  yesterdayUnplannedAbsences: any[] = [];
  yesterdayShortWorkingHours: any[] = [];
  lastWeekUnplannedAbsences: any[] = [];
  lastWeekShortWorkingHours: any[] = [];

  filteredYesterdayAbsences: any[] = [];
  filteredYesterdayShortHours: any[] = [];
  filteredLastWeekAbsences: any[] = [];
  filteredLastWeekShortHours: any[] = [];

  yesterdayAbsenceSort: { column: SortColumn | '', direction: SortDirection } = { column: '', direction: '' };
  yesterdayShortSort: { column: SortColumn | '', direction: SortDirection } = { column: '', direction: '' };
  lastWeekAbsenceSort: { column: SortColumn | '', direction: SortDirection } = { column: '', direction: '' };
  lastWeekShortSort: { column: SortColumn | '', direction: SortDirection } = { column: '', direction: '' };

  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  
  isLoading: boolean = false;
  
  itemSize: number = 48;
  maxHeight: number = 600;
  Math = Math;

  @ViewChild('yesterdayAbsenceViewport') yesterdayAbsenceViewport: CdkVirtualScrollViewport;
  @ViewChild('yesterdayShortViewport') yesterdayShortViewport: CdkVirtualScrollViewport;
  @ViewChild('lastWeekAbsenceViewport') lastWeekAbsenceViewport: CdkVirtualScrollViewport;
  @ViewChild('lastWeekShortViewport') lastWeekShortViewport: CdkVirtualScrollViewport;
  
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
  
  filterBranch(): void {
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
      return 'Deselect All';
    } else if (selectedIds && selectedIds.length > 0) {
      return 'Deselect';
    } else {
      return 'Select All';
    }
  }

  onSelectOpened(opened: boolean): void {
    if (!opened) {
      this.branchSearchText = '';
      this.filterBranch();
    }
  }

  onTabChange(event: MatTabChangeEvent): void {
    this.selectedTabIndex = event.index;
    if (this.selectedTabIndex === 0) {
      this.selectedBranchIds = [...this.selectedBranchIdsYesterday];
    } else {
      this.selectedBranchIds = [...this.selectedBranchIdsLastWeek];
    }
    setTimeout(() => {
      if (this.yesterdayAbsenceViewport) this.yesterdayAbsenceViewport.checkViewportSize();
      if (this.yesterdayShortViewport) this.yesterdayShortViewport.checkViewportSize();
      if (this.lastWeekAbsenceViewport) this.lastWeekAbsenceViewport.checkViewportSize();
      if (this.lastWeekShortViewport) this.lastWeekShortViewport.checkViewportSize();
    });
  }
  
  refresh(): void {
    this.loadYesterdayReport();
    this.loadLastWeekReport();
  }
  
  onBranchSelectionChange(): void {
    if (this.selectedTabIndex === 0) {
      this.selectedBranchIds = [...this.selectedBranchIdsYesterday];
      this.loadYesterdayReport();
    } else {
      this.selectedBranchIds = [...this.selectedBranchIdsLastWeek];
      this.loadLastWeekReport();
    }
  }
  
  loadYesterdayReport(): void {
    this.isLoading = true;
    
    this.anomaliesReportService.getAnomaliesTimelogReport(this.selectedBranchIdsYesterday)
      .subscribe({
        next: (data) => {
          this.yesterdayReportData = data;
          this.processYesterdayData(data);
          this.applySearchFilter();
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
          this.applySearchFilter();
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
      .filter(a => a.notes === 'No leave/WFH record')
      .map(a => ({ ...a, date: this.parseDateFromString(a.date) || a.date }));

    this.yesterdayShortWorkingHours = (data.yesterdayAnomalies || [])
      .filter(a => a.notes === 'No early leave/late arrival approval')
      .map(a => ({ ...a, date: this.parseDateFromString(a.date) || a.date }));

    console.log('Processed Yesterday Unplanned Absences:', this.yesterdayUnplannedAbsences);
    console.log('Processed Yesterday Short Working Hours:', this.yesterdayShortWorkingHours);

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
      const day = parseInt(parts[0], 10);
      const month = parseInt(parts[1], 10) - 1;
      const year = parseInt(parts[2], 10);
      if (!isNaN(day) && !isNaN(month) && !isNaN(year)) {
        return new Date(year, month, day);
      }
    }
    const parsed = Date.parse(dateStr);
    return isNaN(parsed) ? null : new Date(parsed);
  }
  
  onSearchChange(): void {
    this.applySearchFilter();
  }

  clearSearch(): void {
    this.searchText = '';
    this.applySearchFilter();
  }

  applySearchFilter(): void {
    const search = this.searchText.toLowerCase().trim();

    const filterFn = (items: any[]) => {
      if (!search) return [...items];
      return items.filter(item =>
        item.employeeName.toLowerCase().includes(search) ||
        item.branch.toLowerCase().includes(search) ||
        item.userName.toLowerCase().includes(search)
      );
    };

    this.filteredYesterdayAbsences = filterFn(this.yesterdayUnplannedAbsences);
    this.filteredYesterdayShortHours = filterFn(this.yesterdayShortWorkingHours);
    this.filteredLastWeekAbsences = filterFn(this.lastWeekUnplannedAbsences);
    this.filteredLastWeekShortHours = filterFn(this.lastWeekShortWorkingHours);
  }

  sort(column: SortColumn, dataType: 'yesterday' | 'lastWeek', tableType: 'absence' | 'short'): void {
    const sortState = this.getSortState(dataType, tableType);
    
    if (sortState.column === column) {
      if (sortState.direction === 'asc') {
        sortState.direction = 'desc';
      } else if (sortState.direction === 'desc') {
        sortState.direction = '';
        sortState.column = '';
      }
    } else {
      sortState.column = column;
      sortState.direction = 'asc';
    }

    this.applySort(dataType, tableType);
  }

  getSortState(dataType: 'yesterday' | 'lastWeek', tableType: 'absence' | 'short'): { column: SortColumn | '', direction: SortDirection } {
    if (dataType === 'yesterday') {
      return tableType === 'absence' ? this.yesterdayAbsenceSort : this.yesterdayShortSort;
    }
    return tableType === 'absence' ? this.lastWeekAbsenceSort : this.lastWeekShortSort;
  }

  applySort(dataType: 'yesterday' | 'lastWeek', tableType: 'absence' | 'short'): void {
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

      if (column === 'branch') {
        valA = a.branch.branchName || '';
        valB = b.branch.branchName || '';
      } else if (column === 'actualHours' || column === 'count') {
        valA = parseFloat(a[column]) || 0;
        valB = parseFloat(b[column]) || 0;
        return direction === 'asc' ? valA - valB : valB - valA;
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
    if (dataType === 'yesterday') {
      return tableType === 'absence'
        ? this.yesterdayUnplannedAbsences
        : this.yesterdayShortWorkingHours;
    }
    return tableType === 'absence'
      ? this.lastWeekUnplannedAbsences
      : this.lastWeekShortWorkingHours;
  }

  getFilteredArray(dataType: string, tableType: string): any[] {
    if (dataType === 'yesterday') {
      return tableType === 'absence'
        ? this.filteredYesterdayAbsences
        : this.filteredYesterdayShortHours;
    }
    return tableType === 'absence'
      ? this.filteredLastWeekAbsences
      : this.filteredLastWeekShortHours;
  }

  updateFilteredArray(dataType: string, tableType: string, data: any[]): void {
    if (dataType === 'yesterday') {
      if (tableType === 'absence') this.filteredYesterdayAbsences = data;
      else this.filteredYesterdayShortHours = data;
    } else {
      if (tableType === 'absence') this.filteredLastWeekAbsences = data;
      else this.filteredLastWeekShortHours = data;
    }
  }
  
  getSortIcon(column: SortColumn, dataType: 'yesterday' | 'lastWeek', tableType: 'absence' | 'short'): string {
    const sortState = this.getSortState(dataType, tableType);
    if (sortState.column !== column) {
      return 'unfold_more';
    }
    return sortState.direction === 'asc' ? 'arrow_upward' : 'arrow_downward';
  }
}