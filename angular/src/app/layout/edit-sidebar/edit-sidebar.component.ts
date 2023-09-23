import { Component, OnInit, Injector, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-edit-sidebar',
  templateUrl: './edit-sidebar.component.html',
  styleUrls: ['./edit-sidebar.component.css']
})
export class EditSidebarComponent extends AppComponentBase implements OnInit {
  menuItems = {}
  constructor(inject  : Injector, @Inject(MAT_DIALOG_DATA) public data: any) {
    super(inject)
   }

  ngOnInit() {
      this.menuItems = this.data;
      
  }

}
