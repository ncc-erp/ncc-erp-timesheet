import { Component, Input, Output, EventEmitter, ViewChild, OnChanges, SimpleChanges, AfterViewInit } from '@angular/core';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { DataType, TableType } from '../anomalies-report.component';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { SelectAllText } from '../enum/anomalies-report.enum';

@Component({
  selector: 'app-anomalies-tab',
  templateUrl: './anomalies-tab.component.html',
  styleUrls: ['../anomalies-report.component.css']
})
export class AnomaliesTabComponent implements OnChanges, AfterViewInit {
  @Input() dataType: DataType;
  @Input() searchText: string;
  @Input() selectedBranchIds: number[];
  @Input() filteredAbsences: any[] = [];
  @Input() filteredShortHours: any[] = [];
  @Input() isLoading: boolean;
  @Input() itemSize: number = 48;
  @Input() maxHeight: number = 600;
  @Input() filteredBranches: BranchDto[] = [];
  @Input() isActive: boolean = false;
  @Input() selectedTabIndex: number = 0;
  @Input() listBranch: BranchDto[] = [];
  @Input() getSortIconFn: (column: string, dataType: DataType, tableType: TableType) => string;
  @Input() isAllSelectedFn: () => boolean;

  @Output() searchChange = new EventEmitter<void>();
  @Output() searchChangeEmit = new EventEmitter<{ dataType: DataType; searchText: string }>();
  @Output() branchChange = new EventEmitter<{ dataType: DataType; selectedBranchIds: number[] }>();
  @Output() sortEmit = new EventEmitter<{ column: string, dataType: DataType, tableType: TableType }>();
  @Output() selectAllToggle = new EventEmitter<MouseEvent>();
  @Output() filterBranchEmit = new EventEmitter<string>();
  @Output() clearSearchEmit = new EventEmitter<void>();

  branchSearchText: string = '';
  
  public DataType = DataType;
  public TableType = TableType;
  public Math = Math;

  @ViewChild('absenceViewport') absenceViewport: CdkVirtualScrollViewport;
  @ViewChild('shortViewport') shortViewport: CdkVirtualScrollViewport;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.isActive && changes.isActive.currentValue === true) {
      setTimeout(() => this.checkViewports(), 0);
    }
    
    if (changes.filteredAbsences || changes.filteredShortHours) {
      if (this.isActive) {
        setTimeout(() => this.checkViewports(), 0);
      }
    }
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.checkViewports(), 100);
  }

  checkViewports(): void {
    if (this.absenceViewport) {
      this.absenceViewport.checkViewportSize();
    }
    if (this.shortViewport) {
      this.shortViewport.checkViewportSize();
    }
  }

  onSearchChange(): void {
    this.searchChangeEmit.emit({ dataType: this.dataType, searchText: this.searchText });
  }

  clearSearch(): void {
    this.clearSearchEmit.emit();
  }

  filterBranch(): void {
    this.filterBranchEmit.emit(this.branchSearchText);
  }

  onBranchSelectionChange(): void {
    this.branchChange.emit({ dataType: this.dataType, selectedBranchIds: this.selectedBranchIds });
  }

  toggleSelectAll(event: MouseEvent): void {
    this.selectAllToggle.emit(event);
  }

  sort(column: string, dataType: DataType, tableType: TableType): void {
    this.sortEmit.emit({ column, dataType, tableType });
  }

  getSortIcon(column: string, dataType: DataType, tableType: TableType): string {
    if (this.getSortIconFn) {
      return this.getSortIconFn(column, dataType, tableType);
    }
    return 'unfold_more';
  }

  isAllSelected(): boolean {
    return this.listBranch.length > 0 &&
             this.selectedBranchIds.length === this.listBranch.length;
  }

  getSelectAllText(): string {
    if (this.isAllSelected()) {
      return SelectAllText.DESELECT_ALL;
    } else if (this.selectedBranchIds && this.selectedBranchIds.length > 0) {
      return SelectAllText.DESELECT;
    } else {
      return SelectAllText.SELECT_ALL;
    }
  }
}