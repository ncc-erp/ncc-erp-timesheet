import { Component, Output, EventEmitter, Input } from '@angular/core';

@Component({
  selector: 'file-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.css']
})
export class UploadComponent {

  public progress: number;
  public message: string;

  @Input() custom: string; // danger!!! don't delete me. 'toại'
  @Input() title: string;
  @Input() fileType: string;
  @Input() page: string;
  @Output() outputImgBase64: EventEmitter<File> = new EventEmitter();
  @Output() outputFileInfo: EventEmitter<File> = new EventEmitter();
  @Input() isLoading: boolean = false;

  onFileSelected(event) {
    if (event.target.files && event.target.files[0]) {
      const reader = new FileReader();

      reader.readAsDataURL(event.target.files[0]); // read file as data url upload File
      this.outputFileInfo.emit(event.target.files[0]);
      
      if (event.target.files && event.target.files[0].type.includes('image')) {
        reader.onload = event => {
          // called once readAsDataURL is completed. Preview Image
          const item: any = event;
          this.outputImgBase64.emit(item.target.result);
        };
      }
      // event.target.files = [];
    }
  }



  getFileType(): string {
    switch (this.fileType) {
      case 'image':
        return 'image/*';
      case 'video':
        return 'video/*';
      case 'application':
        return 'application/*';
      default:
        return this.fileType;
    }
  }
}
