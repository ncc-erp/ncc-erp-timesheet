import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  SimpleChange,
  ViewChild,
} from "@angular/core";
import { MatSelect, MatSelectChange } from "@angular/material";
import * as _ from "lodash";

@Component({
  selector: "app-multiple-select-new",
  templateUrl: "./multiple-select-new.component.html",
  styleUrls: ["./multiple-select-new.component.css"],
})
export class MultipleSelectNewComponent implements OnInit {
  @ViewChild("matSelect") matSelect: MatSelect;
  @Input() labelName: string;
  @Input() listOption: optionDto[] = [];
  @Input() defaultValue?: number[] = [];
  @Input() disabled?: boolean;
  @Input() classCustom?: string;

  @Output() onChange = new EventEmitter<any>();

  listFilteredOption: optionDto[] = [];

  listSelectedId: number[] = [];
  listOptionBySearch: optionDto[] = [];
  listOptionSelect: optionDto[] = [];

  textSearch: string = "";
  isSearch: boolean = false;

  constructor() {}

  ngOnChanges(change: SimpleChange) {
    if (change['listOption']) {
      this.listOptionBySearch = change["listOption"].currentValue;
    }
    if (change['defaultValue']) {
      this.listSelectedId = change["defaultValue"].currentValue;
    }
  }

  ngOnInit(): void {}

  handleSearch(value: string) {
    this.textSearch = value;
    this.listOptionBySearch = this.listOption;
    if (this.textSearch) {
      this.listOptionBySearch = this.listOption.filter((item) =>
        item.name.toLowerCase().includes(this.textSearch.toLowerCase())
      );
    } else {
      this.listOptionBySearch = this.listOption;
    }
  }

  handleChangeValue(e: MatSelectChange) {
    this.listSelectedId = e.value;
  }

  handleSelectAll() {
    this.listSelectedId = [];
    for (let i = 0; i < this.listOptionBySearch.length; i++) {
      this.listSelectedId.push(this.listOptionBySearch[i].id);
    }
  }

  handleClear() {
    this.listSelectedId = [];
  }

  onOkSelect() {
    this.matSelect.close();
    this.onChange.emit(this.listSelectedId);
  }
}
export class optionDto {
  id: number;
  name: string;
}
