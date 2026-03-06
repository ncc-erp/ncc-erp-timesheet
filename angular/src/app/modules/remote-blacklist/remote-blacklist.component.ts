import { Component, Injector, OnInit, ViewEncapsulation } from '@angular/core';
import { trigger, style, animate, transition } from '@angular/animations';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { MatDialog } from '@angular/material/dialog';
import { AddUserToRemoteBlacklistDialogComponent } from './add-user-to-remote-blacklist-dialog/add-user-to-remote-blacklist-dialog.component';
import { ImportCsvDialogComponent } from './import-csv-dialog/import-csv-dialog.component';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { RemoteBlacklistService } from '@app/service/api/remote-blacklist.service';
import { GetRemoteBlacklistDto, UpdatePenaltyDaysDto } from '@app/service/api/model/remote-blacklist.dto';
import * as FileSaver from 'file-saver';
import { RemoteBlacklistColumn, SortDirection } from './enum/remote-blacklist.enum';

@Component({
  selector: 'app-remote-blacklist',
  templateUrl: './remote-blacklist.component.html',
  styleUrls: ['./remote-blacklist.component.css'],
  animations: [
    appModuleAnimation(), 
    trigger('fade', [
        transition(':enter', [ 
            style({ opacity: 0 }),
            animate('300ms ease-out', style({ opacity: 1 }))
        ]),
        transition(':leave', [ 
            animate('300ms ease-in', style({ opacity: 0 }))
        ])
    ])
  ],
  encapsulation: ViewEncapsulation.None
})
export class RemoteBlacklistComponent extends PagedListingComponentBase<GetRemoteBlacklistDto> implements OnInit {

  VIEW_REMOTE_BLACKLIST = PERMISSIONS_CONSTANT.ViewRemoteBlacklist;
  ADD_REMOTE_BLACKLIST = PERMISSIONS_CONSTANT.AddRemoteBlacklist;
  EDIT_REMOTE_BLACKLIST = PERMISSIONS_CONSTANT.EditRemoteBlacklist;
  DELETE_REMOTE_BLACKLIST = PERMISSIONS_CONSTANT.DeleteRemoteBlacklist;
  IMPORT_REMOTE_BLACKLIST = PERMISSIONS_CONSTANT.ImportRemoteBlacklist;
  DOWNLOAD_REMOTE_BLACKLIST_TEMPLATE = PERMISSIONS_CONSTANT.DownloadTemplateRemoteBlacklist;
  editingRowId: number | null = null;
  dataSource: GetRemoteBlacklistDto[] = [];
  originalDataSource: GetRemoteBlacklistDto[] = [];
  tempPenaltyDays: number = 0;
  maxAllowedRemoteDays: number = 0;
  displayedColumns: string[] = [
    RemoteBlacklistColumn.UserId,
    RemoteBlacklistColumn.FullName,
    RemoteBlacklistColumn.UserName,
    RemoteBlacklistColumn.PenaltyDays,
    RemoteBlacklistColumn.Actions
  ];
  sortProperty: string = '';
  sortDirection: SortDirection = SortDirection.None;
  localSortDirectionEnum = SortDirection;
  RemoteBlacklistColumn = RemoteBlacklistColumn;

  constructor(
    injector: Injector,
    private _dialog: MatDialog,
    private remoteBlacklistService: RemoteBlacklistService
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.getMaxRemoteDays();
    this.refresh();
  }

  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    this.remoteBlacklistService.getAll(request).subscribe(
      (res) => {
        this.dataSource = [...res.result.items];
        this.originalDataSource = [...res.result.items];
        this.showPaging(res.result, pageNumber);
        finishedCallback();
      },
      (error) => {
        finishedCallback();
      }
    );
  }

  protected delete (item: GetRemoteBlacklistDto): void {
    throw new Error('Method not implemented.');
  }

  onSearchChange(): void {
    if (!this.originalDataSource) {
      return;
    }

    let search = this.searchText;
    if (!search) {
      this.dataSource = this.originalDataSource;
      return;
    } else {
      search = search.toLowerCase().trim();
    }

    this.dataSource = this.originalDataSource.filter(user =>
      (user.userName && user.userName.toLowerCase().indexOf(search) > -1) ||
      (user.fullName && user.fullName.toLowerCase().indexOf(search) > -1)
    );
  }

  clearSearch(): void {
    this.searchText = '';
    this.dataSource = this.originalDataSource;
    if (this.sortProperty && this.sortDirection !== SortDirection.None) {
      this.sortData();
    }
  }

  onSortChange(property: string): void {
    if (this.sortProperty === property) {
      if (this.sortDirection === SortDirection.Ascending) {
        this.sortDirection = SortDirection.Descending;
      } else if (this.sortDirection === SortDirection.Descending) {
        this.sortDirection = SortDirection.None;
      } else {
        this.sortDirection = SortDirection.Ascending;
      }
    } else {
      this.sortProperty = property;
      this.sortDirection = SortDirection.Ascending;
    }
    this.sortData();
  }

  sortData(): void {
    if (this.sortDirection === SortDirection.None) {
      this.dataSource = [...this.originalDataSource];

      if (this.searchText) {
        const search = this.searchText.toLowerCase().trim();
        this.dataSource = this.dataSource.filter(user =>
          (user.userName && user.userName.toLowerCase().indexOf(search) > -1) ||
          (user.fullName && user.fullName.toLowerCase().indexOf(search) > -1)
        );
      }
      return;
    }

    if (!this.dataSource || this.dataSource.length === 0) {
      return;
    }

    const isDesc = this.sortDirection === this.localSortDirectionEnum.Descending;

    this.dataSource.sort((a, b) => {
      let valueA: any;
      let valueB: any;

      switch (this.sortProperty) {
        case RemoteBlacklistColumn.FullName:
          valueA = a.fullName ? a.fullName.toLowerCase() : '';
          valueB = b.fullName ? b.fullName.toLowerCase() : '';
          break;
        case RemoteBlacklistColumn.UserName:
          valueA = a.userName ? a.userName.toLowerCase() : '';
          valueB = b.userName ? b.userName.toLowerCase() : '';
          break;
        case RemoteBlacklistColumn.PenaltyDays:
          valueA = a.penaltyDays;
          valueB = b.penaltyDays;
          break;
        default:
          return 0;
      }

      if (valueA < valueB) {
        return isDesc ? 1 : -1;
      } else if (valueA > valueB) {
        return isDesc ? -1 : 1;
      } else {
        return 0;
      }
    });
  }

  addNew(): void {
    const dialogRef = this._dialog.open(AddUserToRemoteBlacklistDialogComponent, {
      disableClose: true,
      width: '600px',
      data: {
        maxAllowedRemoteDays: this.maxAllowedRemoteDays
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.dataSource = [result.result, ...this.dataSource];
        this.totalItems = (this.totalItems || 0) + 1;
      }
    });
  }

  importCsv(): void {
    const dialogRef = this._dialog.open(ImportCsvDialogComponent, {
      disableClose: true,
      width: '450px',
      data: {} 
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.refresh();
      }
    });
  }

  private convertFile(fileData) {
    var buf = new ArrayBuffer(fileData.length);
    var view = new Uint8Array(buf);
    for (var i = 0; i != fileData.length; ++i)
      view[i] = fileData.charCodeAt(i) & 0xff;
    return buf;
  }

  downloadTemplate(): void {
    this.remoteBlacklistService.downloadTemplate().subscribe((rs) => {
    const file = new Blob([this.convertFile(atob(rs.result.base64))], {
      type: "application/vnd.ms-excel;charset=utf-8",
    });
    FileSaver.saveAs(file, "TemplateImportRemoteBlacklist.xlsx");
  });
  }

  enableEdit(item: GetRemoteBlacklistDto): void {
    this.editingRowId = item.id;
    this.tempPenaltyDays = item.penaltyDays;
  }

  cancelEdit(): void {
    this.editingRowId = null;
    this.tempPenaltyDays = 0;
  }

  getMaxRemoteDays() {
    this.remoteBlacklistService.getMaxRemoteDays().subscribe(res => {
      this.maxAllowedRemoteDays = res.result;
    });
  }

  increaseDays(): void {
    if (this.tempPenaltyDays < this.maxAllowedRemoteDays) {
      this.tempPenaltyDays++;
    }
  }

  decreaseDays(): void {
    if (this.tempPenaltyDays > 0) {
      this.tempPenaltyDays--;
    }
  }

  saveEdit(item: GetRemoteBlacklistDto): void {
    if (!this.permission.isGranted(this.EDIT_REMOTE_BLACKLIST)) {
      this.notify.error('Unauthorized');
      return;
    }
    const input = new UpdatePenaltyDaysDto(item.id, this.tempPenaltyDays);
    this.remoteBlacklistService.update(input).subscribe((result: any) => {
      if (result) {
        const data = result.result;
        this.notify.success('Update Successfully');
        item.penaltyDays = data.penaltyDays;
        this.editingRowId = null;
      }
    }, () => {
      this.notify.error('Update Failed');
    });
  }

  editItem(item: GetRemoteBlacklistDto): void {
    if (!this.permission.isGranted(this.EDIT_REMOTE_BLACKLIST)) {
      this.notify.warn('Unauthorized');
      return;
    }
    this.enableEdit(item);
  }

  deleteItem(item: GetRemoteBlacklistDto): void {
    if (!this.permission.isGranted(this.DELETE_REMOTE_BLACKLIST)) {
      this.notify.warn('Unauthorized');
      return;
    }
    abp.message.confirm(
      `Do you want to remove user ${item.userName} from the remote blacklist?`, 
      "Are you sure?",
      (isConfirmed) => {
        if (isConfirmed) {
          this.remoteBlacklistService.delete(item.id).subscribe(
            (res: boolean) => {
              if (res) {
                this.notify.success('Delete Successfully');
                this.refresh();
              } else {
                this.notify.error('Delete Failed');
              }
            },
            (err) => {
              this.notify.error('Delete Failed');
            }
          );
        }
      }
    );
  }
}