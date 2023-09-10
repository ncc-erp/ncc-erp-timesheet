import {
  MatDialogRef,
} from "@node_modules/@angular/material";
import { TeamBuildingPMService } from "./../../../../service/api/team-building-pm.service";
import { Component, OnInit, Injector } from "@angular/core";
import * as _ from "lodash";
import {
  InvoiceRequestDto,
  PMRequestDto,
  RequestDto,
  SelectTeamBuildingDetailDto,
  SelectUserOtherProjectDto,
} from "../../const/const";
import { CreateEditRequestComponentBase } from "../../CreateEditRequest/CreateEditRequestComponentBase";
import { AbstractControl, FormArray, FormBuilder, FormControl, FormGroup, ValidationErrors, ValidatorFn, Validators } from "@angular/forms";

export function invoiceFormCustomValidator() : ValidatorFn {
  return (form : FormGroup) : ValidationErrors | null => {
    const invoiceFileName : any = form.get("invoiceFileName").value;
    const invoiceUrl : any = form.get("invoiceUrl").value;
    if(invoiceFileName || invoiceUrl) {
      return null;
    } else {
      return { fileAndUrlIsAllNull : true };
    }
  }
}

@Component({
  selector: "app-pm-send-request",
  templateUrl: "./pm-send-request.component.html",
  styleUrls: ["./pm-send-request.component.css"],
})
export class PmSendRequestComponent extends CreateEditRequestComponentBase implements OnInit {

  public listSelectedItemUserOtherProjectId: number [] = [];
  public invoiceFormGroup: FormGroup = new FormGroup({});

  constructor(
    injector: Injector,
    public teamBuildingPMService: TeamBuildingPMService,
    public dialogRef: MatDialogRef<PmSendRequestComponent>,
    private formBuilder : FormBuilder
  ) {
    super(injector, teamBuildingPMService);
  }

  ngOnInit() {
    this.getAllRequest();
    this.getAllProject();
    this.getListBranch();
    this.fileReader.onloadend = () => {
      var base64data = this.fileReader.result;
      this.fileUploadList.push(base64data);
    };
    this.initInvoiceFormArray();
  }

  initInvoiceFormArray() {
    this.invoiceFormGroup = this.formBuilder.group({
      note : new FormControl(null, Validators.required),
      totalInvoiceAmount : new FormControl({value : "", disabled : true}),
      invoiceFormArray : this.formBuilder.array([this.createInvoiceForm()])
    });
  }

  createInvoiceForm() {
    return this.formBuilder.group({
      invoiceFileName : new FormControl({value : null, disabled: false}),
      invoiceFile : new FormControl(null),
      isChoosingFile : new FormControl(true),
      invoiceUrl : new FormControl({value : null, disabled: true}),
      amount : new FormControl(0, Validators.min(1000)),
      isVat : new FormControl(false)
    }, {
      validators: [invoiceFormCustomValidator()]
    });
  }

  addNewInvoiceForm() {
    const formArray = this.invoiceFormGroup.get("invoiceFormArray") as FormArray;
    formArray.push(this.createInvoiceForm());
  }

  deleteInvoiceForm(index : number) {
    const formArray = this.invoiceFormGroup.get("invoiceFormArray") as FormArray;
    formArray.removeAt(index);
    this.calculateTotalInvoiceAmount();
  }

  switchInvoiceTypeResource(index : number) {
    const formArray = this.invoiceFormGroup.get("invoiceFormArray") as FormArray;
    const formGroup = formArray.at(index) as FormGroup;
    const isChoosingFileValue = formGroup.controls["isChoosingFile"].value;
    if (isChoosingFileValue) {
      formGroup.controls['invoiceFileName'].enable();
      formGroup.controls['invoiceUrl'].disable();
      formGroup.controls['invoiceUrl'].setValue(null);
    } else {
      formGroup.controls['invoiceFileName'].disable();
      formGroup.controls['invoiceFileName'].setValue(null);
      formGroup.controls['invoiceFile'].setValue(null);
      formGroup.controls['invoiceUrl'].enable();
    }
  }

  onFileChange(event: any, index: number) {
    const selectedFile = event.target.files[0]; // Get the selected file
    const formArray = this.invoiceFormGroup.get("invoiceFormArray") as FormArray;
    
    if (formArray.at(index)) {
      const invoiceForm = formArray.at(index) as FormGroup;
      invoiceForm.controls["invoiceFile"].setValue(selectedFile); // Update the FormControl's value with the selected file
    }
  }

  calculateTotalInvoiceAmount() {
    let totalInvoiceAmount = this.invoiceFormGroup.get("totalInvoiceAmount") as FormControl;
    totalInvoiceAmount.setValue("0");
    const formArray = this.invoiceFormGroup.get("invoiceFormArray") as FormArray;
    for(let control of formArray.controls) {
      if(control instanceof FormGroup) {
        const amountValue = control.controls["amount"].value as number;
        const totalMoney = parseFloat(totalInvoiceAmount.value) + amountValue;
        totalInvoiceAmount.setValue(totalMoney.toString());
      }
    }
    this.onInputInvoiceAmountChange();
  }

  // Hàm Get Data
  getAllRequest(): void {
    this.isLoading = true;

    this.teamBuildingPMService
      .getAllRequest(this.selectedProjectId, this.month, this.branchId)
      .subscribe(
        (res) => {
          const requestAdding = this.requestAdding.filter(
            (item) =>
              (!this.selectedProjectId || item.projectId === this.selectedProjectId) &&
              (!this.month || +(item.applyMonth.split("-")[1].split("T")[0]) === this.month) &&
              (!this.branchId || item.branchId === this.branchId)
          );
          this.requestInfo = [...res.result.teamBuildingDetailDtos, ...requestAdding];
          this.originalList = [...res.result.teamBuildingDetailDtos, ...requestAdding];
          this.lastRemainMoney = res.result.lastRemainMoney;
          this.isLoading = false;
          this.isFirst && this.setAll(false);
          this.isFirst = false;
        },
        () => (this.isLoading = false)
      );
  }

  getRequestMoneyInfoUser() {
    let requestInfoByUser: RequestDto[] = [];
    this.teamBuildingPMService
      .GetRequestMoneyInfoUser(this.listSelectedItemUserOtherProjectId)
      .subscribe((res) => {
        requestInfoByUser = res.result.map((t) => {
          return {
            ...t,
            selected: true,
            isNotInProject: true,
          };
        });
        this.requestAdding = [...this.requestAdding, ...requestInfoByUser];
        this.listSelectedItem = [...this.listSelectedItem, ...requestInfoByUser.map(item => {
          return {
            id: item.id,
            money: item.money,
            status: item.status,
            selected: true
          } as SelectTeamBuildingDetailDto
        })];

        this.listSelectedItemUserOtherProjectId = [];
        this.listSelectedItemUserOtherProject = [];

        this.getAllRequest();
      });
  }

  handleSelectRequestInfoItem(index, $event) {
    this.requestInfo[index].selected = $event.checked;

    const item = this.listSelectedItem.find(
      (item) => item.id == this.requestInfo[index].id
    );
    if (item) {
      item.selected = $event.checked;
    } else {
      this.listSelectedItem.push({
        selected: $event.checked,
        id: this.requestInfo[index].id,
        money: this.requestInfo[index].money,
        status: this.requestInfo[index].status
      });
    }

    this.updateAllComplete();
    this.checkSelectedCheckbox();
  }

  onChangeUserIdSelected(event) {
    this.listSelectedItemUserOtherProject = event;
    this.getRequestMoneyInfoUser();
  }

  updateAllComplete() {
    this.allComplete =
      this.requestInfo != null &&
      this.requestInfo.every((t) => this.getIsSelected(t));
    this.getTotalMoney();
  }

  onSaveAndClose() {

    this.saving = true;
    let listIds = this.listSelectedItem.filter((x) => x.selected).map((x) => x.id);

    // Get Invoice Request from FormArray, then push to InvoiceRequestDto
    let invoiceRequestDtoList : InvoiceRequestDto[] = [];
    let fileArray : Map<number, File> = new Map();

    const formArray = this.invoiceFormGroup.get("invoiceFormArray") as FormArray;
    let count = 0;
    for(let invoiceForm of formArray.controls) {
      if(invoiceForm instanceof FormGroup) {

        let invoiceRequestDto : InvoiceRequestDto = new InvoiceRequestDto();
        const isChoosingFile = invoiceForm.controls["isChoosingFile"].value;

        if(isChoosingFile) {
          fileArray.set(count, invoiceForm.controls["invoiceFile"].value);
          invoiceRequestDto.invoiceImageName = count.toString() + "." + invoiceForm.controls["invoiceFileName"].value.split(".")[1];
        }

        invoiceRequestDto.invoiceUrl = invoiceForm.controls["invoiceUrl"].value;
        invoiceRequestDto.amount = invoiceForm.controls["amount"].value;
        invoiceRequestDto.hasVat = invoiceForm.controls["isVat"].value;
        invoiceRequestDtoList.push(invoiceRequestDto);

        count++;

      }
    } 

    const noteControl = this.invoiceFormGroup.get("note") as FormControl;
    const totalInvoiceAmount = this.invoiceFormGroup.get("totalInvoiceAmount") as FormControl;

    const request: PMRequestDto = {
      titleRequest: "",
      listDetailId: listIds,
      
      note: noteControl.value,
      invoiceAmount: totalInvoiceAmount.value,

      listInvoiceRequestDto : invoiceRequestDtoList,
      
    };
    this.requestAdding = [];

    this.teamBuildingPMService.addDataToTeamBuildingDetail(request, fileArray).subscribe(
      (response) => {
        if (response) {
          abp.notify.success("PM send request successful");
          this.saving = false;
          this.dialogRef.close(true);
        }
      },
      () => (this.saving = false)
    );
  }

  onInputInvoiceAmountChange(): void {
    const totalInvoiceAmount = this.invoiceFormGroup.get("totalInvoiceAmount") as FormControl;
    const numSelections = Math.floor(parseFloat(totalInvoiceAmount.value));
    const listIds = this.getListSuggestSelectedItemIds(this.requestInfo.filter(s => s.status == 0), numSelections);

    for(let i = 0; i < this.requestInfo.length; i++) {
      this.handleSelectRequestInfoItem(i, {checked: listIds.indexOf(this.requestInfo[i].id) != -1} );
    }
  }

  getListSuggestSelectedItemIds(objectList: RequestDto[], totalMoney): number[] {
    // Sắp xếp danh sách object theo tiền (money) giảm dần
    const sortedList = objectList.sort((a, b) => b.money - a.money);
    const selectedItems = [];
    let currentTotal = 0;

    for (let i = 0; i < sortedList.length; i++) {
      const obj = sortedList[i];
      if (currentTotal + obj.money <= totalMoney) {
        selectedItems.push(obj);
        currentTotal += obj.money;
      }
    }
    return selectedItems.map(s => s.id);
  }

  getSelectedUserOtherProject(value: SelectUserOtherProjectDto[]){
    this.listSelectedItemUserOtherProjectId = value.map( item => item.id);
    this.getRequestMoneyInfoUser();
  }

  closePopup(value: boolean) {
    this.isShowBtnAddUser = value;
  }

  close(d) {
    this.dialogRef.close(d);
  }
}


