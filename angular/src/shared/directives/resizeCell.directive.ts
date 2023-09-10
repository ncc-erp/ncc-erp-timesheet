import { Directive, ElementRef, HostListener, Input, OnInit } from "@angular/core";
@Directive({
    selector: '[resizeCell]'
})
export class ResizeCellDirective implements OnInit {
    @Input() maxLine = 1;
    constructor(private el: ElementRef<HTMLElement>) {
    }
    ngOnInit(): void {
        this.el.nativeElement.style.webkitBoxOrient = 'vertical'
    }
    @HostListener('click', ['$event'])
    public handleMouseClick(eventTarget): void{
        this.el.nativeElement.classList.toggle(`max-line-content-${this.maxLine}`)
    }
}