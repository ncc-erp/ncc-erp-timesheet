import { Component, OnInit, Injector } from "@angular/core";
import { MatDialog, MatSelectChange } from "@angular/material";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";
import {
  EnumSortName,
  RetroActionDialogData,
  RetroDto,
  RetroSort,
} from "@app/service/api/model/retro-dto";
import { RetroService } from "@app/service/api/retro.service";
import { ActionDialog } from "@shared/AppEnums";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import { CreateEditRetroComponent } from "./create-edit-retro/create-edit-retro.component";
import { finalize } from "rxjs/operators";
import { EnumSort } from "@app/service/api/model/retro-detail-dto";

enum ActionEnum {
  Close = "close",
  Edit = "edit",
  Delete = "delete",
  Check = "check",
}

enum StatusEnum {
  Publish = 0,
  Close = 1,
  All = 2,
}

interface Action {
  icon: ActionEnum;
  title: string;
  permissions?: any;
}

export interface status {
  title: string;
  value: StatusEnum;
}

@Component({
  selector: "app-retro",
  templateUrl: "./retro.component.html",
  styleUrls: ["./retro.component.css"],
})
export class RetroComponent
  extends PagedListingComponentBase<RetroDto>
  implements OnInit
{
  VIEW = PERMISSIONS_CONSTANT.ViewRetro;
  VIEWDETAILAllTEAM = PERMISSIONS_CONSTANT.RetroDetail_ViewAllTeam;
  VIEWDETAILMYTEAM = PERMISSIONS_CONSTANT.RetroDetail_ViewMyTeam;
  ADD = PERMISSIONS_CONSTANT.AddNewRetro;
  EDIT = PERMISSIONS_CONSTANT.EditRetro;
  DELETE = PERMISSIONS_CONSTANT.DeleteRetro;
  CHANGESTATUS = PERMISSIONS_CONSTANT.ChangeStatus;

  public listRetro: RetroDto[] = [];

  public listOptionDate: string[] = [];

  public listStatus: status[] = [
    { value: StatusEnum.All, title: "All" },
    { value: StatusEnum.Publish, title: "Active" },
    { value: StatusEnum.Close, title: "Inactive" },
  ];

  public status: StatusEnum = StatusEnum.All;
  public sortByStartDate: RetroSort = {
    name: EnumSortName.StartDate,
    value: EnumSort.NotArranged,
  };

  public statusTime: string = "All";

  constructor(
    injector: Injector,
    private dialog: MatDialog,
    private retroService: RetroService
  ) {
    super(injector);
  }

  ngOnInit() {
    this.refresh();
  }

  create(): void {
    let item = {} as RetroDto;
    this.showDialog(item, ActionDialog.CREATE);
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    request.filterItems = [];
    if (this.searchText) {
      request.searchText = this.searchText;
    }
    if (this.sortByStartDate.value !== EnumSort.NotArranged) {
      request.sort = this.sortByStartDate.name;
      request.sortDirection = this.sortByStartDate.value;
    }
    if (this.status !== StatusEnum.All) {
      request.filterItems.push({
        propertyName: "status",
        value: this.status,
        comparison: 0,
      });
    }

    this.retroService
      .getAllPagging(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.listRetro = result.result.items;
        this.showPaging(result.result, pageNumber);
      });
  }

  protected delete(item: RetroDto): void {
    abp.message.confirm(`Delete ${item.name}?`, (result: boolean) => {
      if (result) {
        this.retroService.delete(item.id).subscribe(() => {
          abp.notify.info("Deleted retro successfully");
          this.refresh();
        });
      }
    });
  }

  public handleAction(type: ActionEnum, item: RetroDto) {
    if (type === ActionEnum.Delete) {
      this.delete(item);
    } else if (type === ActionEnum.Edit) {
      this.showDialog(item, ActionDialog.EDIT);
    } else {
      this.retroService.changeStatus(item.id).subscribe(() => {
        abp.notify.info("Change status retro successfully");
        this.refresh();
      });
    }
  }

  public handleShowAction() {
    if (
      this.permission.isGranted(this.DELETE) ||
      this.permission.isGranted(this.CHANGESTATUS) ||
      this.permission.isGranted(this.EDIT)
    ) {
      return true;
    }
    return false;
  }

  public handleShowLinkRetroDetail() {
    return (
      this.permission.isGranted(this.VIEWDETAILAllTEAM) ||
      this.permission.isGranted(this.VIEWDETAILMYTEAM)
    );
  }

  public handleChangeStatus(e: MatSelectChange) {
    this.status = e.value;
    this.refresh();
  }

  handleSortStartDate() {
    if (this.sortByStartDate.value === EnumSort.NotArranged) {
      this.sortByStartDate.value = EnumSort.ASC;
    } else if (this.sortByStartDate.value === EnumSort.ASC) {
      this.sortByStartDate.value = EnumSort.DEC;
    } else {
      this.sortByStartDate.value = EnumSort.NotArranged;
    }
    this.refresh();
  }

  showDialog(retro: RetroDto, action: ActionDialog): void {
    const { id, startDate, endDate, deadline, name, status } = retro;
    let item = {
      id,
      name,
      startDate,
      endDate,
      deadline,
      status,
    } as RetroDto;
    const dialogRef = this.dialog.open(CreateEditRetroComponent, {
      data: {
        item: item,
        action: action,
      } as RetroActionDialogData,
      disableClose: true,
      width: "700px",
    });

    dialogRef.afterClosed().subscribe((res) => {
      if (res) {
        const firstPage = 1;
        const pageNumber = retro.id == null ? firstPage : this.pageNumber;
        this.getDataPage(pageNumber);
      }
    });
  }
}
