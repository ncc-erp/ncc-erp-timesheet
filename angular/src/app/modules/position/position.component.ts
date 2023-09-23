import { CreateEditPositionComponent } from "./create-edit-position/create-edit-position.component";
import { MatDialog } from "@angular/material";
import { Component, OnInit, Injector } from "@angular/core";
import {
  PagedListingComponentBase,
  PagedRequestDto,
} from "@shared/paged-listing-component-base";
import { finalize } from "rxjs/operators";
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";
import { PositionService } from "@app/service/api/position.service";
import {
  PositionCreateEditDto,
  PositionDialogData,
  PositionDto,
} from "@app/service/api/model/position-dto";
import { ActionDialog } from "@shared/AppEnums";

@Component({
  selector: "app-position",
  templateUrl: "./position.component.html",
  styleUrls: ["./position.component.css"],
})
export class PositionComponent
  extends PagedListingComponentBase<PositionDto>
  implements OnInit
{
  VIEW = PERMISSIONS_CONSTANT.ViewPosition;
  ADD = PERMISSIONS_CONSTANT.AddNewPosition;
  EDIT = PERMISSIONS_CONSTANT.EditPosition;
  DELETE = PERMISSIONS_CONSTANT.DeletePosition;
  listPosition = [] as PositionDto[];
  constructor(
    private positionService: PositionService,
    private dialog: MatDialog,
    injector: Injector
  ) {
    super(injector);
  }

  ngOnInit() {
    this.refresh();
  }

  protected list(
    request: PagedRequestDto,
    pageNumber: number,
    finishedCallback: Function
  ): void {
    if (this.searchText) {
      request.searchText = this.searchText;
    }

    this.positionService
      .getAllPagging(request)
      .pipe(
        finalize(() => {
          finishedCallback();
        })
      )
      .subscribe((result: any) => {
        this.listPosition = result.result.items;
        this.showPaging(result.result, pageNumber);
      });
  }

  protected delete(item: PositionDto): void {
    abp.message.confirm(
      "Delete position '" + item.name + "'?",
      (result: boolean) => {
        if (result) {
          this.positionService.delete(item.id).subscribe(() => {
            abp.notify.info("Deleted position " + item.name);
            this.refresh();
          });
        }
      }
    );
  }

  create(): void {
    let item = {} as PositionCreateEditDto;
    this.showDialog(item, ActionDialog.CREATE);
  }

  edit(position: PositionCreateEditDto): void {
    this.showDialog(position, ActionDialog.EDIT);
  }

  showDialog(position: PositionCreateEditDto, action: ActionDialog): void {
    const { id, name, shortName, code, color } = position;
    let item = {
      id,
      name,
      shortName,
      code,
      color,
    } as PositionCreateEditDto;
    const dialogRef = this.dialog.open(CreateEditPositionComponent, {
      data: {
        item: item,
        action: action,
      } as PositionDialogData,
      disableClose: true,
      width: "800px",
    });

    dialogRef.afterClosed().subscribe((res) => {
      if (res) {
        const firstPage = 1;
        const pageNumber = position.id == null ? firstPage : this.pageNumber;
        this.getDataPage(pageNumber);
      }
    });
  }
}
