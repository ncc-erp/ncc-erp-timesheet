import { Component, Inject, Injector, OnInit, ViewEncapsulation } from "@angular/core";
import { UserWhitelistService } from "@app/service/api/user-whitelist.service";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";
import { GetUserWhitelistDto } from "@app/service/api/model/user-whitelist.dto";
import { PagedListingComponentBase } from "@shared/paged-listing-component-base";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import { UserWhitelistColumn, SortDirection, SortDirectionIcon } from "./enum/user-whitelist.enum";
import { MatDialog } from "@angular/material";
import { AddEditUserWhitelistComponent } from "./add-edit-user-whitelist/add-edit-user-whitelist.component";
import { WhitelistType } from "@app/service/api/model/whitelist-system.dto";
import { WhitelistSystemService } from "@app/service/api/whitelist-system.service";
import * as FileSaver from "file-saver";
import { ImportExcelComponent } from "./import-excel/import-excel.component";

@Component({
    selector: 'app-user-whitelist',
    templateUrl: './user-whitelist.component.html',
    styleUrls: ['./user-whitelist.component.css'],
    animations: [appModuleAnimation()]
})
export class UserWhitelistComponent extends PagedListingComponentBase<GetUserWhitelistDto> implements OnInit {
  
    VIEW_USERWHITELIST = PERMISSIONS_CONSTANT.ViewUserWhitelist;
    ADD_USERWHITELIST = PERMISSIONS_CONSTANT.AddUserWhitelist;
    DELETE_USERWHITELIST = PERMISSIONS_CONSTANT.DeleteUserWhitelist;
    IMPORT_USERWHITELIST = PERMISSIONS_CONSTANT.ImportUserWhitelist;
    DOWNLOAD_USERWHITELIST_TEMPLATE = PERMISSIONS_CONSTANT.DownloadTemplateUserWhitelist;

    UserWhitelistColumn = UserWhitelistColumn;
    sortColumn: UserWhitelistColumn | '' = '';
    sortDirection: SortDirection = SortDirection.None;

    allData: GetUserWhitelistDto[] = [];
    
    searchText: string = '';
    whitelistTypesList: WhitelistType[] = [];
    selectedType: number = -1;

    constructor(
        injector: Injector,
        private userWhitelistService: UserWhitelistService,
        private whitelistSystemService: WhitelistSystemService,
        private dialog: MatDialog
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.fetchWhitelistTypes();
        this.refresh();
    }

    protected list(request: any, pageNumber: number, finishedCallback: Function): void {
        this.userWhitelistService.getAll().subscribe((response: any) => {
            let items: GetUserWhitelistDto[] = response.result ? response.result : response;

            if (this.selectedType !== null && this.selectedType !== undefined && this.selectedType !== -1) {
                items = items.filter(x => x.whitelistType === this.selectedType);
            }
            
            if (this.searchText) {
                const searchLower = this.searchText.toLowerCase().trim();
                items = items.filter(x => 
                    (x.userName || '').toLowerCase().includes(searchLower)
                );
            }

            if (this.sortColumn && this.sortDirection !== SortDirection.None) {
                items.sort((a: any, b: any) => {
                    let valA: any;
                    let valB: any;

                    if (this.sortColumn === UserWhitelistColumn.Branch) {
                        valA = a.branch ? a.branch.branchName : '';
                        valB = b.branch ? b.branch.branchName : '';
                    } else {
                        valA = a[this.sortColumn] || '';
                        valB = b[this.sortColumn] || '';
                    }
                    
                    if (typeof valA === 'string') valA = valA.toLowerCase();
                    if (typeof valB === 'string') valB = valB.toLowerCase();

                    if (valA < valB) return this.sortDirection === SortDirection.Ascending ? -1 : 1;
                    if (valA > valB) return this.sortDirection === SortDirection.Ascending ? 1 : -1;
                    return 0;
                });
            }

            this.totalItems = items.length;
            this.pageNumber = pageNumber;
            const pagedItems = items.slice((this.pageNumber - 1) * this.pageSize, this.pageNumber * this.pageSize);
            this.allData = pagedItems.map(item => ({
                ...item,
                isExpanded: false 
            }));
            finishedCallback();
        }, () => {
            finishedCallback();
        });
    }

    onSort(column: UserWhitelistColumn): void {
        if (this.sortColumn === column) {
            if (this.sortDirection === SortDirection.Ascending) {
                this.sortDirection = SortDirection.Descending;
            } else if (this.sortDirection === SortDirection.Descending) {
                this.sortDirection = SortDirection.None;
                this.sortColumn = '';
            } else {
                this.sortDirection = SortDirection.Ascending;
            }
        } else {
            this.sortColumn = column;
            this.sortDirection = SortDirection.Ascending;
        }
        this.getDataPage(1);
    }
    
    getSortIcon(column: UserWhitelistColumn): string {
        if (this.sortColumn !== column || this.sortDirection === SortDirection.None) {
            return SortDirectionIcon.None;
        }
        return this.sortDirection === SortDirection.Ascending ? SortDirectionIcon.Up : SortDirectionIcon.Down;
    }

    onSearchChange(): void {
        this.getDataPage(1);
    }

    onTypeChange(): void {
        this.getDataPage(1);
    }

    clearSearch(): void {
        this.searchText = '';
        this.getDataPage(1);
    }

    fetchWhitelistTypes() {
        this.whitelistSystemService.getWhitelistTypes().subscribe((res: any) => {
            this.whitelistTypesList = res.result ? res.result : res;
        });
    }

    toggleProjects(item: any): void {
        if (item.projectNames && item.projectNames.length > 2) {
            item.isExpanded = !item.isExpanded;
        }
    }

    addNewUser(): void {
        this.showDialog();
    }

    deleteItem(item: GetUserWhitelistDto): void {
        this.delete(item);
    }

    protected delete (item: GetUserWhitelistDto): void {
        abp.message.confirm(
            this.l('Are you sure you want to delete this user with type: "' + item.whitelistName + '" from this whitelist?'),
            this.l('Confirm Delete'),
            (result: boolean) => {
                if (result) {
                    this.userWhitelistService.delete(item.id).subscribe(() => {
                        abp.notify.success(this.l('Deleted successfully'));
                        this.getDataPage(this.pageNumber);
                    });
                }
            }
        )
    }

    private showDialog(): void {
        const dialogRef = this.dialog.open(AddEditUserWhitelistComponent, {
            width: '40%',
            maxWidth: '800px',
            panelClass: 'user-whitelist-dialog-container',
            restoreFocus: false,
            data: {
                whitelistTypesList: this.whitelistTypesList
            }
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.getDataPage(1);
            }
        });
    }

    importUsers(): void {
        const dialogRef = this.dialog.open(ImportExcelComponent, {
            disableClose: true,
            width: '30%',
            autoFocus: false,
            restoreFocus: false,
            data: {}
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.getDataPage(1);
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
        this.userWhitelistService.downloadTemplate().subscribe((rs) => {
            const file = new Blob([this.convertFile(atob(rs.result.base64))], {
                type: "application/vnd.ms-excel;charset=utf-8",
            });
            FileSaver.saveAs(file, "TemplateImportWhitelist.xlsx");
        })
    }
}