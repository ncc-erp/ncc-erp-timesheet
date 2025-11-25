import { MmnClient, ZkClient } from '@node_modules/mmn-client-js/dist';
import { AppConsts } from '@shared/AppConsts';

let _mmnClient: MmnClient | null = null;
let _zkClient: ZkClient | null = null;

function createMmnClient(): MmnClient {
  return new MmnClient({
    baseUrl:  AppConsts.buildMmnUrl('mmn-api/'),
  });
}

function createZkClient(): ZkClient {
  return new ZkClient({
    endpoint: AppConsts.buildMmnUrl('zk-api'),
  });
}

export const mmnClient: MmnClient = new Proxy({} as MmnClient, {
  get(target, prop) {
    if (!_mmnClient) {
      _mmnClient = createMmnClient();
    }
    const value = _mmnClient[prop as keyof MmnClient];
    return typeof value === 'function' ? value.bind(_mmnClient) : value;
  }
});

export const zkClient: ZkClient = new Proxy({} as ZkClient, {
  get(target, prop) {
    if (!_zkClient) {
      _zkClient = createZkClient();
    }
    const value = _zkClient[prop as keyof ZkClient];
    return typeof value === 'function' ? value.bind(_zkClient) : value;
  }
});

