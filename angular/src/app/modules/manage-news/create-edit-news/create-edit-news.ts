import { AppComponentBase } from '@shared/app-component-base';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { Component, OnInit, Optional, Injector, Inject } from '@angular/core';
import { ManageNewsService } from '@app/service/api/manage-new.service';
import { AngularEditorConfig } from '@kolkov/angular-editor';
import { NewsDto, newsdto } from '@app/service/api/model/news-dto';

@Component({
  selector: 'app-create-edit-news',
  templateUrl: './create-edit-news.html',
  styleUrls: ['./create-edit-news.css']
})
export class CreateEditNewsComponent extends AppComponentBase implements OnInit {
  news = {} as NewsDto;
  newNews: newsdto;
  title: string;
  config: AngularEditorConfig = {
    editable: true,
    spellcheck: true,
    height: '15rem',
    minHeight: '5rem',
    placeholder: 'Enter text here...',
    translate: 'no',
    toolbarPosition: 'top',
    defaultFontName: 'Times New Roman',
    customClasses: [
      {
        name: "quote",
        class: "quote",
      },
      {
        name: 'redText',
        class: 'redText'
      },
      {
        name: "titleText",
        class: "titleText",
        tag: "h1",
      },
    ]
  };

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    injector: Injector,
    private manageNewsService: ManageNewsService
  ) {
    super(injector);
  }

  ngOnInit() {
    this.newNews = this.data;
    if (this.newNews.id == undefined) {
      this.title = "Create News";
    } else {
      this.title = "Edit News";
      this.manageNewsService.get(this.newNews.id).subscribe(res => {
        this.newNews = res.result;
        console.log('asdasdas', this.newNews);
      });
    }
  }


}
