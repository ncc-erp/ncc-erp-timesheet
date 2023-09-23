import { Component, Input, OnInit } from '@angular/core';
import { SortDirectionEnum } from '@shared/AppEnums';

@Component({
  selector: 'sortable-header',
  templateUrl: './sortable-header.component.html',
  styleUrls: ['./sortable-header.component.css']
})
export class SortableHeaderComponent implements OnInit {

  constructor() { }
  @Input() sortProperty: string = "";
  @Input() sortDirection: number
  @Input() name: string = ""
  public sortDirectionEnum = SortDirectionEnum;

  ngOnInit() {
  }

}
