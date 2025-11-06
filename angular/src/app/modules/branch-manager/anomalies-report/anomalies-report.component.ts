// anomalies-report.component.ts
import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { AnomaliesReportService, AnomaliesTimelogReportResponse } from '@app/service/api/anomalies-report.service';
import { BranchDto } from '@shared/service-proxies/service-proxies';

type SortColumn = 'employeeName' | 'branch' | 'actualHours' | 'count';
type SortDirection = 'asc' | 'desc';

@Component({
  selector: 'app-anomalies-report',
  templateUrl: './anomalies-report.component.html',
  styleUrls: ['./anomalies-report.component.css']
})
export class AnomaliesReportComponent implements OnInit {
  selectedTabIndex: number = 0;
  
  @Input() listBranch: BranchDto[] = [];
  @Input() listBranchFilter: BranchDto[] = [];
  
  selectedBranchIds: number[] = [];
  branchSearchText: string = '';
  searchText: string = '';
  
  reportData: AnomaliesTimelogReportResponse | null = null;
  
  yesterdayUnplannedAbsences: any[] = [];
  yesterdayShortWorkingHours: any[] = [];
  lastWeekUnplannedAbsences: any[] = [];
  lastWeekShortWorkingHours: any[] = [];

  filteredYesterdayAbsences: any[] = [];
  filteredYesterdayShortHours: any[] = [];
  filteredLastWeekAbsences: any[] = [];
  filteredLastWeekShortHours: any[] = [];

  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  
  isLoading: boolean = false;
  
  itemSize: number = 48;
  maxHeight: number = 600;
  Math = Math;
  
  constructor(private anomaliesReportService: AnomaliesReportService) {}
  
  ngOnInit(): void {
    if (this.listBranchFilter && this.listBranchFilter.length > 0) {
      this.selectedBranchIds = this.listBranchFilter.map(b => b.id);
    } else if (this.listBranch && this.listBranch.length > 0) {
      this.selectedBranchIds = this.listBranch.map(b => b.id);
      this.listBranchFilter = [...this.listBranch];
    }
    
    if (this.selectedBranchIds.length > 0) {
      this.loadReport();
    }
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
    if (this.isAllSelected()) {
      this.selectedBranchIds = [];
    } else {
      this.selectedBranchIds = this.listBranch.map(b => b.id);
    }
    this.loadReport();
  }

  isAllSelected(): boolean {
    return this.listBranch.length > 0 &&
           this.selectedBranchIds.length === this.listBranch.length;
  }

  onSelectOpened(opened: boolean): void {
    if (!opened) {
      this.branchSearchText = '';
      this.filterBranch();
    }
  }

  onTabChange(event: MatTabChangeEvent): void {
    this.selectedTabIndex = event.index;
  }
  
  refresh(): void {
    this.loadReport();
  }
  
  onBranchSelectionChange(): void {
    this.loadReport();
  }
  
  loadReport(): void {
    if (this.selectedBranchIds.length === 0) {
      this.clearData();
      return;
    }
    
    this.isLoading = true;
    
    this.anomaliesReportService.getAnomaliesTimelogReport(this.selectedBranchIds)
      .subscribe({
        next: (data) => {
          this.processData(data);
          this.applySearchFilter();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading report:', error);
          this.isLoading = false;
        }
      });
  }
  
  clearData(): void {
    this.yesterdayUnplannedAbsences = [];
    this.yesterdayShortWorkingHours = [];
    this.lastWeekUnplannedAbsences = [];
    this.lastWeekShortWorkingHours = [];
    this.filteredYesterdayAbsences = [];
    this.filteredYesterdayShortHours = [];
    this.filteredLastWeekAbsences = [];
    this.filteredLastWeekShortHours = [];
  }

  processData(data: AnomaliesTimelogReportResponse): void {
    this.yesterdayUnplannedAbsences = (data.yesterdayAnomalies || [])
      .filter(a => a.notes === 'No leave/WFH record')
      .map(a => ({ ...a, date: this.parseDateFromString(a.date) || a.date }));

    this.yesterdayShortWorkingHours = (data.yesterdayAnomalies || [])
      .filter(a => a.notes === 'No early leave/late arrival approval')
      .map(a => ({ ...a, date: this.parseDateFromString(a.date) || a.date }));

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

    this.filteredYesterdayAbsences = [...this.yesterdayUnplannedAbsences];
    this.filteredYesterdayShortHours = [...this.yesterdayShortWorkingHours];
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
        item.branch.toLowerCase().includes(search)
      );
    };

    this.filteredYesterdayAbsences = filterFn(this.yesterdayUnplannedAbsences);
    this.filteredYesterdayShortHours = filterFn(this.yesterdayShortWorkingHours);
    this.filteredLastWeekAbsences = filterFn(this.lastWeekUnplannedAbsences);
    this.filteredLastWeekShortHours = filterFn(this.lastWeekShortWorkingHours);
  }

  sort(column: SortColumn, dataType: 'yesterday' | 'lastWeek', tableType: 'absence' | 'short'): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }

    const data = this.getDataArray(dataType, tableType);
    const filtered = this.getFilteredArray(dataType, tableType);

    const direction = this.sortDirection === 'asc' ? 1 : -1;

    filtered.sort((a: any, b: any) => {
      let valA = (a[column] !== null && a[column] !== undefined) ? a[column] : '';
      let valB = (b[column] !== null && b[column] !== undefined) ? b[column] : '';

      if (column === 'actualHours' || column === 'count') {
        valA = parseFloat(valA) || 0;
        valB = parseFloat(valB) || 0;
        return (valA - valB) * direction;
      }

      return valA.toString().localeCompare(valB.toString()) * direction;
    });

    this.updateFilteredArray(dataType, tableType, filtered);
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
  
  getSortIcon(column: string): string {
    if (this.sortColumn !== column) {
      return 'unfold_more';
    }
    return this.sortDirection === 'asc' ? 'arrow_upward' : 'arrow_downward';
  }
}