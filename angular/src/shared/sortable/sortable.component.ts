import { Component, EventEmitter, Injector, Input, OnInit, Output } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-sortable',
  templateUrl: './sortable.component.html',
  styleUrls: ['./sortable.component.css']
})
export class SortableComponent extends AppComponentBase implements OnInit {

  @Input() data: any;
  @Output() sortEvent = new EventEmitter<SortableModel>();
  public childValue: SortableModel;
  constructor(injector: Injector) {
    super(injector)
  }

  ngOnInit(): void {
    this.childValue = this.data
  }
  handleClick(typeSort){
    if(typeSort == ''){
      this.childValue.typeSort = 'ASC'
      this.childValue.sortDirection = 0
    }
    else if(typeSort == 'ASC'){
      this.childValue.typeSort = 'DESC'
      this.childValue.sortDirection = 1
    }
    else{
      this.childValue.typeSort = 'ASC'
      this.childValue.sortDirection = 0
    }
    
    this.sortEvent.next(this.data)
  }

}
export class SortableModel{
  constructor(_sort, _sortDirection, _typeSort){
    this.sort = _sort
    this.sortDirection = _sortDirection
    this.typeSort = _typeSort
  }
  sortDirection: any
  sort: string
  typeSort: string
}