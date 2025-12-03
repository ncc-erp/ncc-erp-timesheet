import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
  ElementRef,
  AfterViewInit,
  ViewChild,
} from "@angular/core";
import {
  DailyProjectTimelogReportService,
  DailyProjectTimelogReportResponse,
} from "@app/service/api/daily-project-report.service";
import { BranchDto } from "@shared/service-proxies/service-proxies";
import { SortColumn, SortDirection, SelectAllText, SortArrow } from './enum/daily-project-report.enum';
import { CdkVirtualScrollViewport } from '@node_modules/@angular/cdk/scrolling';

declare var ResizeObserver: any;

interface ProjectReportItem {
  name: string;
  members: string[];
  totalTimelogLW: number;
  totalTimelogLM: number;
  memberCount: number;
  expanded?: boolean;
}

@Component({
  selector: "app-daily-project-report",
  templateUrl: "./daily-project-report.component.html",
  styleUrls: ["./daily-project-report.component.css"],
})
export class DailyProjectReportComponent implements OnInit, OnChanges, AfterViewInit {
  @Input() listBranch: BranchDto[];
  @Input() listBranchFilter: BranchDto[];

  branchIds: number[] = [];
  branchSearchText: string = "";
  searchText: string = "";
  minHours: number;
  limit: number;

  projects: ProjectReportItem[] = [];
  filteredProjects: ProjectReportItem[] = [];
  itemSize: number = 48;
  maxHeight = 400;

  sortColumn: SortColumn = SortColumn.None;
  sortDirection: SortDirection = SortDirection.None;

  reportData: DailyProjectTimelogReportResponse[];
  isLoading: boolean = false;

  Math = Math;

  @ViewChild('scrollViewport') scrollViewport: CdkVirtualScrollViewport;

  private hasSetItemSize: boolean = false;

  constructor(
    private dailyProjectReportService: DailyProjectTimelogReportService,
    private elementRef: ElementRef
  ) {}

  ngOnInit(): void {
    if (!this.listBranchFilter) {
      this.listBranchFilter = this.listBranch || [];
    }
    this.searchOrFilter();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.listBranch && changes.listBranch.currentValue) {
      this.listBranchFilter = changes.listBranch.currentValue;
      if (this.filteredProjects) {
        setTimeout(() => this.checkViewports(), 0);
      }
    }
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.checkViewports(), 100);
  }

  checkViewports(): void {
    if (this.scrollViewport) {
      this.scrollViewport.checkViewportSize();
    }
  }

  searchOrFilter(): void {
    this.isLoading = true;
    const selectedBranchIds = this.branchIds || [];
    const isAllBranch = selectedBranchIds.length === 0 || selectedBranchIds.length === this.listBranch.length;
    const branchIdsToSend = isAllBranch ? [] : selectedBranchIds;

    this.dailyProjectReportService
      .getDailyProjectTimelogReport(
        branchIdsToSend,
        this.minHours,
        this.limit,
        undefined,
        isAllBranch
      )
      .subscribe({
        next: (response) => {
          this.reportData = response;
          this.projects = (response || []).map((p) => ({
            ...p,
            memberCount: p.members ? p.members.length : 0,
            expanded: false,
          }));
          this.applyFilters();
          this.isLoading = false;
          setTimeout(() => {
            this.checkViewports();
            this.setDynamicItemSize();
          }, 0);
        },
        error: (error) => {
          console.error("API Error:", error);
          this.projects = [];
          this.filteredProjects = [];
          this.isLoading = false;
        },
      });
  }

  private setDynamicItemSize(): void {
    if (this.hasSetItemSize || !this.scrollViewport || this.filteredProjects.length === 0) {
      return;
    }
    const rowElement = this.scrollViewport.elementRef.nativeElement.querySelector('tr');
    if (rowElement) {
      this.itemSize = rowElement.offsetHeight;
      this.hasSetItemSize = true;
      this.checkViewports();
    }

  }

  getBranchCodes(): number[] {
    if (
      !this.branchIds ||
      this.branchIds.length === 0
    ) {
      return this.listBranch ? this.listBranch.map((branch) => branch.id) : [];
    }
    return this.branchIds;
  }

  filterBranch(): void {
    if (!this.branchSearchText) {
      this.listBranchFilter = this.listBranch;
    } else {
      const searchText = this.branchSearchText.toLowerCase();
      this.listBranchFilter = this.listBranch.filter(
        (branch) => branch.name.toLowerCase().indexOf(searchText) > -1
      );
    }
  }

  applyFilters(): void {
    let result = [...this.projects];

    if (this.searchText && this.searchText.trim() !== "") {
      const search = this.searchText.toLowerCase().trim();
      result = result.filter(
        (item) =>
          item.name.toLowerCase().includes(search) ||
          item.members.some((m) => m.toLowerCase().includes(search))
      );
    }

    if (this.sortColumn && this.sortDirection) {
      result = this.sortData(result, this.sortColumn, this.sortDirection);
    }

    this.filteredProjects = result;
  }

  sortData(
    data: ProjectReportItem[],
    column: SortColumn,
    direction: SortDirection
  ): ProjectReportItem[] {
    if (!direction) return data;

    return [...data].sort((a, b) => {
      let valueA: any = a[column] || 0;
      let valueB: any = b[column] || 0;

      if (column === SortColumn.Name) {
        valueA = valueA.toLowerCase();
        valueB = valueB.toLowerCase();
        return direction === SortDirection.Asc
          ? valueA.localeCompare(valueB)
          : valueB.localeCompare(valueA);
      }

      return direction === SortDirection.Asc ? valueA - valueB : valueB - valueA;
    });
  }

  onSort(column: SortColumn): void {
    const currentCol = this.sortColumn as SortColumn;
    const currentDir = this.sortDirection as SortDirection;

    if (currentCol === column) {
      if (currentDir === SortDirection.Asc) {
        this.sortDirection = SortDirection.Desc;
      } else if (currentDir === SortDirection.Desc) {
        this.sortDirection = SortDirection.None;
        this.sortColumn = SortColumn.None;
      }
    } else {
      this.sortColumn = column as SortColumn;
      this.sortDirection = SortDirection.Asc;
    }

    this.applyFilters();
  }

  getSortIcon(column: SortColumn): string {
    if (this.sortColumn !== column) return SortArrow.NONE;
    return this.sortDirection === SortDirection.Asc ? SortArrow.UP : SortArrow.DOWN;
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  clearSearch(): void {
    this.searchText = "";
    this.applyFilters();
  }

  onMinHoursEnter(): void {
    const hasValue = this.minHours !== null && this.minHours !== undefined && !isNaN(Number(this.minHours));

    if (hasValue) {
      this.searchOrFilter();
    } else {
      this.minHours = undefined;
      this.searchOrFilter();
    }
  }

  onLimitEnter(): void {
    const hasValue = this.limit !== null && this.limit !== undefined && !isNaN(Number(this.limit));

    if (hasValue) {
      this.searchOrFilter();
    } else {
      this.limit = undefined;
      this.searchOrFilter();
    }
  }

  isAllSelected(): boolean {
    return (
      this.branchIds &&
      this.listBranch &&
      this.branchIds.length === this.listBranch.length
    );
  }

  getSelectAllText(): string {
    if (this.isAllSelected()) {
      return SelectAllText.DESELECT_ALL;
    } else if (this.branchIds && this.branchIds.length > 0) {
      return SelectAllText.DESELECT;
    } else {
      return SelectAllText.SELECT_ALL;
    }
  }
  toggleSelectAll(event?: MouseEvent): void {
    if (event) {
      event.stopPropagation();
    }

    if (this.isAllSelected() || (this.branchIds && this.branchIds.length > 0)) {
      this.branchIds = [];
    } else {
      this.branchIds = this.listBranch.map(b => b.id);
    }
    
    this.searchOrFilter();
  }

  onBranchSelectionChange(selectedIds: number[]): void {
    this.branchIds = selectedIds || [];
    this.searchOrFilter();
  }

  clearAllFilters(): void {
    this.searchText = "";
    this.branchSearchText = "";
    this.branchIds = [];
    this.minHours = undefined;
    this.limit = undefined;
    this.sortColumn = SortColumn.None;
    this.sortDirection = SortDirection.None;
    
    this.projects = [];
    this.filteredProjects = [];
  }

  refresh(): void {
    if (this.branchIds && this.branchIds.length > 0) {
      this.searchOrFilter();
    }
  }

  toggleExpanded(item: ProjectReportItem): void {
    item.expanded = !item.expanded;
  }

  get filteredBranches() {
    return this.listBranchFilter.filter(b => b.id && b.id !== 0);
  }

  round(value: number | string | null): string {
    if (value == null) return '0.0';
    const num = typeof value === 'string' ? parseFloat(value) : value;
    return isNaN(num) ? '0.0' : num.toFixed(1);
  }
}