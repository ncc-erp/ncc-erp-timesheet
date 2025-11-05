// anomalies-report.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { MatTabChangeEvent } from '@angular/material/tabs';
import { AnomaliesReportService, AnomaliesTimelogReportResponse } from '@app/service/api/anomalies-report.service';
import { BranchDto } from '@shared/service-proxies/service-proxies';

@Component({
  selector: 'app-anomalies-report',
  templateUrl: './anomalies-report.component.html',
  styleUrls: ['./anomalies-report.component.css']
})
export class AnomaliesReportComponent implements OnInit {
  // Tab management
  selectedTabIndex: number = 0;
  
  // Branches từ component cha
  @Input() listBranch: BranchDto[] = [];
  @Input() listBranchFilter: BranchDto[] = [];
  
  // Branch selection
  selectedBranchIds: number[] = [];
  
  // Report data
  reportData: AnomaliesTimelogReportResponse | null = null;
  
  // Yesterday data
  yesterdayDate: string = '';
  yesterdayUnplannedAbsences: any[] = [];
  yesterdayShortWorkingHours: any[] = [];
  
  // Last week data
  lastWeekStart: string = '';
  lastWeekEnd: string = '';
  lastWeekUnplannedAbsences: any[] = [];
  lastWeekShortWorkingHours: any[] = [];
  
  // Sort configuration
  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  
  // Loading state
  isLoading: boolean = false;
  
  // Virtual scroll settings
  itemSize: number = 48;
  maxHeight: number = 600;
  Math = Math;
  
  constructor(private anomaliesReportService: AnomaliesReportService) {}
  
  ngOnInit(): void {
    // Initialize with all branches selected
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
          this.reportData = data;
          this.processYesterdayData(data);
          this.processLastWeekData(data);
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
  }
  
  processYesterdayData(data: AnomaliesTimelogReportResponse): void {
    this.yesterdayDate = data.yesterday;
    
    // Split yesterday anomalies into two categories
    this.yesterdayUnplannedAbsences = data.yesterdayAnomalies.filter(
      a => a.actualHours === '0' || a.actualHours === '0.00'
    );
    
    this.yesterdayShortWorkingHours = data.yesterdayAnomalies.filter(
      a => a.actualHours !== '0' && a.actualHours !== '0.00'
    );
  }
  
  processLastWeekData(data: AnomaliesTimelogReportResponse): void {
    this.lastWeekStart = data.lastWeekStart;
    this.lastWeekEnd = data.lastWeekEnd;
    
    // Split last week anomalies
    this.lastWeekUnplannedAbsences = data.lastWeekAnomalies.filter(
      a => a.datesMissed && a.datesMissed.length > 0
    );
    
    this.lastWeekShortWorkingHours = data.lastWeekAnomalies.filter(
      a => (a.datesBelowThreshold && a.datesBelowThreshold.length > 0) ||
           (a.datesNoTrackerTime && a.datesNoTrackerTime.length > 0)
    );
  }
  
  sort(column: string, dataType: 'yesterday' | 'lastWeek', tableType: 'absence' | 'short'): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    
    const direction = this.sortDirection === 'asc' ? 1 : -1;
    
    if (dataType === 'yesterday') {
      const data = tableType === 'absence' 
        ? this.yesterdayUnplannedAbsences 
        : this.yesterdayShortWorkingHours;
      
      data.sort((a, b) => {
        if (column === 'actualHours') {
          return (parseFloat(a.actualHours) - parseFloat(b.actualHours)) * direction;
        }
        const valA = a[column] || '';
        const valB = b[column] || '';
        return valA > valB ? direction : -direction;
      });
    } else {
      const data = tableType === 'absence' 
        ? this.lastWeekUnplannedAbsences 
        : this.lastWeekShortWorkingHours;
      
      data.sort((a, b) => {
        if (column === 'count') {
          return (a.count - b.count) * direction;
        }
        const valA = a[column] || '';
        const valB = b[column] || '';
        return valA > valB ? direction : -direction;
      });
    }
  }
  
  getSortIcon(column: string): string {
    if (this.sortColumn !== column) {
      return 'unfold_more';
    }
    return this.sortDirection === 'asc' ? 'arrow_upward' : 'arrow_downward';
  }
}