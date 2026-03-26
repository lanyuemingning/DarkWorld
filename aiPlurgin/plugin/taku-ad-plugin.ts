import { registerPlugin, PluginListenerHandle } from '@capacitor/core';

export interface TakuAdPlugin {
  Init(): Promise<{ success: boolean; message: string }>;
  ShowInterstitial(): Promise<{ success: boolean; message: string }>;
  ShowRewardVideo(): Promise<{ success: boolean; message: string }>;
  addListener(eventName: 'onReward', listenerFunc: (data: any) => void): Promise<PluginListenerHandle>;
}

const TakuAdManager = registerPlugin<TakuAdPlugin>('TakuAdManager');

export default TakuAdManager;