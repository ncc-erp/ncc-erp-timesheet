import { Component, Injector, OnInit, ViewEncapsulation } from "@angular/core";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";
import { AddWhitelistTypeDto, UpdateWhitelistTypeDto, GetWhitelistSystemDto } from "@app/service/api/model/whitelist-system.dto";
import { WhitelistSystemService } from "@app/service/api/whitelist-system.service";
import { PagedListingComponentBase } from "@shared/paged-listing-component-base";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import { MatDialog } from "@angular/material/dialog";
import { CreateEditWhitelistSystemComponent } from "./create-edit-whitelist-system/create-edit-whitelist-system.component";
import { WhitelistSystemColumn, SortDirection, SortDirectionIcon } from "./enum/whitelist-system.enum";

@Component({
    selector: 'app-whitelist-system',
    templateUrl: './whitelist-system.component.html',
    styleUrls: ['./whitelist-system.component.css'],
    animations: [appModuleAnimation()]
})
export class WhitelistSystemComponent extends PagedListingComponentBase<GetWhitelistSystemDto> implements OnInit {

    VIEW_WHITELISTSYSTEM = PERMISSIONS_CONSTANT.ViewWhitelistSystem;
    ADD_WHITELISTSYSTEM = PERMISSIONS_CONSTANT.AddWhitelistSystem;
    EDIT_WHITELISTSYSTEM = PERMISSIONS_CONSTANT.EditWhitelistSystem;
    DELETE_WHITELISTSYSTEM = PERMISSIONS_CONSTANT.DeleteWhitelistSystem;

    WhitelistSystemColumn = WhitelistSystemColumn;

    whitelistTypesList: GetWhitelistSystemDto[] = [];
    searchText: string = '';

    sortColumn: WhitelistSystemColumn | '' = '';
    sortDirection: SortDirection = SortDirection.None;
    
    constructor(
        injector: Injector,
        private whitelistSystemService: WhitelistSystemService,
        private dialog: MatDialog
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.refresh();
    }

    protected list(request: any, pageNumber: number, finishedCallback: Function): void {
        this.whitelistSystemService.getAll().subscribe((response: any) => {
            let items: GetWhitelistSystemDto[] = response.result ? response.result : response;

            if (this.searchText) {
                const searchLower = this.searchText.toLowerCase().trim();
                items = items.filter(x => 
                    (x.name || '').toLowerCase().includes(searchLower) || 
                    (x.description || '').toLowerCase().includes(searchLower)
                );
            }

            if (this.sortColumn && this.sortDirection !== SortDirection.None) {
                items.sort((a: any, b: any) => {
                    let valA = a[this.sortColumn] || '';
                    let valB = b[this.sortColumn] || '';
                    
                    if (typeof valA === 'string') valA = valA.toLowerCase();
                    if (typeof valB === 'string') valB = valB.toLowerCase();

                    if (valA < valB) return this.sortDirection === SortDirection.Ascending ? -1 : 1;
                    if (valA > valB) return this.sortDirection === SortDirection.Ascending ? 1 : -1;
                    return 0;
                });
            }

            this.totalItems = items.length;
            this.whitelistTypesList = items.slice((this.pageNumber - 1) * this.pageSize, this.pageNumber * this.pageSize);

            finishedCallback();
        }, () => {
            finishedCallback();
        });
    }

    onSort(column: WhitelistSystemColumn): void {
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

    getSortIcon(column: WhitelistSystemColumn): string {
        if (this.sortColumn !== column || this.sortDirection === SortDirection.None) {
            return SortDirectionIcon.None;
        }
        return this.sortDirection === SortDirection.Ascending ? SortDirectionIcon.Up : SortDirectionIcon.Down;
    }

    onSearchChange(): void {
        this.getDataPage(1);
    }

    clearSearch(): void {
        this.searchText = '';
        this.getDataPage(1);
    }

    addNew() {
        const newItem = { isActive: true } as AddWhitelistTypeDto;
        this.showDialog(newItem as any);
    }

    editItem(item: UpdateWhitelistTypeDto): void {
        const editItem = { ...item } as UpdateWhitelistTypeDto;
        this.showDialog(editItem);
    }

    deleteItem(item: GetWhitelistSystemDto): void {
        this.delete(item);
    }

    protected delete(item: GetWhitelistSystemDto): void {
        abp.message.confirm(
            this.l('Are you sure you want to delete the whitelist type: ' + item.name + '?'),
            this.l('Confirm Delete'),
            (result: boolean) => {
                if (result) {
                    this.whitelistSystemService.delete(item.id).subscribe(() => {
                        abp.notify.success(this.l('Deleted successfully'));
                        this.refresh();
                    });
                }
            }
        );
    }

    private showDialog(item: AddWhitelistTypeDto | UpdateWhitelistTypeDto): void {
        const dialogRef = this.dialog.open(CreateEditWhitelistSystemComponent, {
            data: item,
            disableClose: true,
            panelClass: 'whitelist-system-dialog-container',
            restoreFocus: false,
            width: '40%',
            maxWidth: '800px'
        });
        
        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                if (!(item as UpdateWhitelistTypeDto).id) {
                    this.getDataPage(1);
                } else {
                    this.getDataPage(this.pageNumber);
                }
            }
        });
    }
}