import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, OnInit, OnDestroy } from '@angular/core';
import { DataType, TableType } from '../anomalies-report.component';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { SelectAllText } from '../enum/anomalies-report.enum';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { YesterdayAnomaly, LastWeekAnomaly } from '@app/modules/branch-manager/Dto/anomalies-report-dto';

@Component({
  selector: 'app-anomalies-tab',
  templateUrl: './anomalies-tab.component.html',
  styleUrls: ['../anomalies-report.component.css']
})
export class AnomaliesTabComponent implements OnChanges, OnInit, OnDestroy {
  @Input() dataType: DataType;
  @Input() searchText: string;
  @Input() selectedBranchIds: number[];
  @Input() filteredAbsences: (YesterdayAnomaly | LastWeekAnomaly)[] = [];
  @Input() filteredShortHours: (YesterdayAnomaly | LastWeekAnomaly)[] = [];
  @Input() isLoading: boolean;
  @Input() filteredBranches: BranchDto[] = [];
  @Input() isActive: boolean = false;
  @Input() selectedTabIndex: number = 0;
  @Input() listBranch: BranchDto[] = [];
  @Input() getSortIconFn: (column: string, dataType: DataType, tableType: TableType) => string;

  @Output() searchChangeEmit = new EventEmitter<{ dataType: DataType; searchText: string }>();
  @Output() branchChange = new EventEmitter<{ dataType: DataType; selectedBranchIds: number[] }>();
  @Output() sortEmit = new EventEmitter<{ column: string, dataType: DataType, tableType: TableType }>();
  @Output() selectAllToggle = new EventEmitter<MouseEvent>();
  @Output() filterBranchEmit = new EventEmitter<string>();
  @Output() clearSearchEmit = new EventEmitter<void>();

  branchSearchText: string = '';
  
  pAbsence: number = 1;
  pageSizeAbsence: number = 10;
  
  pShort: number = 1;
  pageSizeShort: number = 10;

  public DataType = DataType;
  public TableType = TableType;

  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription;

  ngOnInit(): void {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(searchText => {
      this.searchChangeEmit.emit({ dataType: this.dataType, searchText: searchText });
    });
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.filteredAbsences) {
      this.pAbsence = 1;
    }
    if (changes.filteredShortHours) {
      this.pShort = 1;
    }
  }

  onSearchChange(): void {
    this.searchSubject.next(this.searchText);
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
    return 'pi-sort-alt';
  }

  isAllSelected(): boolean {
    return (
      this.selectedBranchIds &&
      this.listBranch &&
      this.selectedBranchIds.length === this.listBranch.length
    );
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