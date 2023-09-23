import { Component, OnInit, Injector, Inject } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { UserService } from '@app/service/api/user.service';
import { ImageCroppedEvent } from 'ngx-image-cropper';

@Component({
  selector: 'app-upload-avatar',
  templateUrl: './upload-avatar.component.html',
  styleUrls: ['./upload-avatar.component.css']
})
export class UploadAvatarComponent extends AppComponentBase implements OnInit {

  isLoading = false;
  imageUrl;
  id;
  response;
  constructor(
    injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private userService: UserService,
    private diaLofRef: MatDialogRef<UploadAvatarComponent>
  ) {
    super(injector);
    this.id = data;
  }

  ngOnInit() {
  }

  title = 'angular-image-uploader';
  file;

  imageChangedEvent: any = '';
  croppedImage: any;

  fileChangeEvent(event: any): void {
    this.imageChangedEvent = event;
  }
  imageCropped(event: ImageCroppedEvent) {
    this.croppedImage = event.base64;
    //this.file = new File([this.dataURItoBlob(this.croppedImage)], "image.jpg");
    if(!navigator.msSaveBlob) {
      this.file = new File([this.dataURItoBlob(this.croppedImage)], "image.jpg");
    } else {
      this.file = this.blobToFile(this.dataURItoBlob(this.croppedImage), "image.jpg");
    }
  }
  imageLoaded() {
    // show cropper
  }
  cropperReady() {
    // cropper ready
  }
  loadImageFailed() {
    // show message
  }

  dataURItoBlob(dataURI): Blob {
    const byteString = atob(dataURI.split(',')[1]);
    const mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];
    const ab = new ArrayBuffer(byteString.length);
    let ia = new Uint8Array(ab);
    for (let i = 0; i < byteString.length; i++) {
      ia[i] = byteString.charCodeAt(i);
    }
    return new Blob([ab], { type: 'image/jpeg' });
  }

  blobToFile(theBlob: Blob, fileName: string): File {
    const b: any = theBlob;
    Object.assign(b, {name: "", lastModified: ""});
    b.lastModifiedDate = new Date();
    b.name = fileName;
    return <File>b;
 }

}
