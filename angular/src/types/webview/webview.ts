import { IMezonWebView, MezonEventHandler, MezonAppEvent } from './types';
export declare class MezonWebView implements IMezonWebView {
  private eventHandlers;
  private locationHash;
  private iFrameStyle;
  initParams: Record<string, string | null>;
  isIframe: boolean;
  constructor();
  postEvent<T>(eventType: string, eventData: T, callback: Function): void;
  receiveEvent<T>(eventType: MezonAppEvent, eventData: T): void;
  onEvent<T>(eventType: MezonAppEvent, callback: MezonEventHandler<T>): void;
  offEvent<T>(eventType: MezonAppEvent, callback: MezonEventHandler<T>): void;
  private initData;
  private initIframe;
  private handleMessage;
  private callEventCallbacks;
}