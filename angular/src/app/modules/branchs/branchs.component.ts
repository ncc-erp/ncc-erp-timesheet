import { CreateEditBranchComponent } from './create-edit-branch/create-edit-branch.component';
import { MatDialog } from '@angular/material';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { BranchService } from '../../service/api/branch.service';
import { Component, OnInit, Injector } from '@angular/core';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { finalize } from 'rxjs/operators';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { ProjectManagerService } from '@app/service/api/project-manager.service';
import { BranchCreateEditDto, BranchDto } from '@shared/service-proxies/service-proxies';

@Component({
  selector: 'app-branchs',
  templateUrl: './branchs.component.html',
  styleUrls: ['./branchs.component.css'],
  animations: [appModuleAnimation()]
})

export class BranchsComponent extends PagedListingComponentBase<BranchDto> implements OnInit {
  VIEW = PERMISSIONS_CONSTANT.ViewBranch;
  ADD = PERMISSIONS_CONSTANT.AddNewBranch;
  EDIT = PERMISSIONS_CONSTANT.EditBranch;
  DELETE = PERMISSIONS_CONSTANT.DeleteBranch;
  branchs = [] as BranchDto[];

  constructor(
    private branchService: BranchService,
    private dialog: MatDialog,
    injector: Injector
  ) {
    super(injector);

  }

  ngOnInit() {
    this.refresh();
  }

  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    if (this.searchText) {
      request.searchText = this.searchText;
    }

    this.branchService
      .getAll(request)
      .pipe(finalize(() => {
        finishedCallback();
      }))
      .subscribe((result: any) => {
        this.branchs = result.result.items;
        this.showPaging(result.result, pageNumber);
      });

  }

  protected delete(item: BranchDto): void {
    abp.message.confirm(
      "Delete branch '" + item.name + "'?",
      (result: boolean) => {
        if (result) {
          this.branchService.delete(item.id).subscribe(() => {
            abp.notify.info('Deleted branch ' + item.name);
            this.refresh();
          });
        }
      }
    );
  }

  create(): void {
    this.showDialog(
      {
        morningWorking: 3.5,
        morningStartAt: "08:30",
        morningEndAt: "12:00",
        afternoonWorking: 4.5,
        afternoonStartAt: "13:00",
        afternoonEndAt: "17:30",
      } as BranchCreateEditDto);
  }

  edit(branch: BranchCreateEditDto): void {
    this.showDialog(branch);
  }

  showDialog(branch: BranchCreateEditDto): void {
    const { id, name, displayName, code, color, morningWorking, morningStartAt, morningEndAt, afternoonWorking, afternoonStartAt, afternoonEndAt } = branch;
    let item = {
      id,
      name,
      displayName,
      code,
      color,
      morningWorking,
      morningStartAt,
      morningEndAt,
      afternoonWorking,
      afternoonStartAt,
      afternoonEndAt
    } as BranchCreateEditDto;
    const dialogRef = this.dialog.open(CreateEditBranchComponent, {
      data: item,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((res) => {
      if (res) {
        const firstPage = 1;
        const pageNumber = branch.id == null ? firstPage : this.pageNumber
        this.getDataPage(pageNumber)
      }
    });
  }
}
