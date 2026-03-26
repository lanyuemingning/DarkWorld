package com.arrow_Away_dai.app;
import android.util.Log;
import android.content.Context;

import com.anythink.core.api.AdError;
import com.getcapacitor.JSObject;
import com.getcapacitor.Plugin;
import com.getcapacitor.PluginCall;
import com.getcapacitor.PluginMethod;
import com.getcapacitor.annotation.CapacitorPlugin;
import com.anythink.core.api.ATAdInfo;
import com.anythink.core.api.ATSDK;
import com.anythink.interstitial.api.ATInterstitialListener;
import com.anythink.interstitial.api.ATInterstitial;
import com.anythink.rewardvideo.api.ATRewardVideoListener;
import com.anythink.rewardvideo.api.ATRewardVideoAd;

/**
 * Taku广告管理器插件
 * 用于管理Anythink SDK的初始化、广告加载和展示
 * 支持激励视频和插屏广告类型
 */
@CapacitorPlugin(name = "TakuAdManager")
public class takuAD extends Plugin {
    private Context context;
    // 日志标志
    private static final String TAG ="TaKu";
    // 临时用倒水的测试
    private static final String APP_ID = "a68229c5f42e2b";
    private static final String APP_KEY = "a82844bf085483dd3018ef16e347250e5";
    
    // 激励视频广告实例
    private ATRewardVideoAd mRewardVideoAd;
    
    // 插屏广告实例
    private ATInterstitial mInterstitialAd;
    
    // 广告是否已初始化加载
    private boolean isAdInitialized = false;
    
    /**
     * 初始化ATSDK
     * 必须在使用广告功能之前调用此方法
     * @param call Capacitor插件调用对象
     */
    @PluginMethod()
    public void Init(PluginCall call) {
        // 获取应用上下文
        context = getContext();

        // 初始化ATSDK
        ATSDK.init(context.getApplicationContext(),APP_ID,APP_KEY);
        
        // 自动加载广告（只执行一次）
        if (!isAdInitialized) {
            initAdLoaders(context);
            isAdInitialized = true;
        }
        
        // 返回初始化成功的结果
        JSObject ret = new JSObject();
        ret.put("success", true);
        ret.put("message", "ATSDK initialized successfully");
        call.resolve(ret);
    }
    

    private void initAdLoaders(Context context) {
        // 初始化激励视频广告
        initRewardVideoAd(context);
        
        // 初始化插屏广告（使用默认的placementId，实际使用时需要替换）
        initInterstitialAd(context);
    }

    private void initRewardVideoAd(Context context) {
        // 固定的激励视频广告位ID
        String placementId = "b1gfnjso5u3hej";
        
        // 创建激励视频广告实例
        mRewardVideoAd = new ATRewardVideoAd(context, placementId);
        
        // 设置激励视频广告监听器
        mRewardVideoAd.setAdListener(new ATRewardVideoListener() {
            @Override
            public void onRewardedVideoAdLoaded() {
                Log.d(TAG, "激励视频广告加载成功");
            }

            @Override
            public void onRewardedVideoAdFailed(AdError adError) {
                Log.d(TAG, "激励视频广告加载失败: " + adError.getCode() + " - " + adError.getDesc());
            }

            @Override
            public void onRewardedVideoAdPlayStart(ATAdInfo atAdInfo) {
                // 广告开始播放，预加载下一次的广告
                Log.d(TAG, "激励视频广告开始播放");
                mRewardVideoAd.load();
                Log.d(TAG, "激励视频广告开始重新加载");
            }

            @Override
            public void onRewardedVideoAdPlayEnd(ATAdInfo atAdInfo) {
                Log.d(TAG, "激励视频广告播放结束");
            }

            @Override
            public void onRewardedVideoAdPlayFailed(AdError adError, ATAdInfo atAdInfo) {
                Log.d(TAG, "激励视频广告播放失败: " + adError.getCode() + " - " + adError.getDesc());
            }

            @Override
            public void onRewardedVideoAdClosed(ATAdInfo atAdInfo) {
                Log.d(TAG, "激励视频广告关闭");
            }

            @Override
            public void onRewardedVideoAdPlayClicked(ATAdInfo atAdInfo) {
                Log.d(TAG, "激励视频广告点击播放");
            }

            @Override
            public void onReward(ATAdInfo atAdInfo) {
                Log.d(TAG, "激励视频广告奖励成功");
                notifyListeners("onReward", new JSObject());
            }
        });
        
        // 开始加载激励视频广告
        mRewardVideoAd.load();
    }

    private void initInterstitialAd(Context context) {
        // 创建插屏广告实例
        mInterstitialAd = new ATInterstitial(context, "b1gfnjso5u3p46");
        
        // 设置插屏广告监听器
        mInterstitialAd.setAdListener(new ATInterstitialListener() {

            @Override
            public void onInterstitialAdLoaded() {
                Log.d(TAG, "插屏广告加载成功");
            }

            @Override
            public void onInterstitialAdLoadFail(AdError adError) {
                Log.d(TAG, "插屏广告加载失败: " + adError.getCode() + " - " + adError.getDesc());
            }

            @Override
            public void onInterstitialAdClicked(ATAdInfo atAdInfo) {
                Log.d(TAG, "插屏广告点击");
            }

            @Override
            public void onInterstitialAdShow(ATAdInfo atAdInfo) {
                Log.d(TAG, "插屏广告展示");
                mInterstitialAd.load();
            }

            @Override
            public void onInterstitialAdClose(ATAdInfo atAdInfo) {
                Log.d(TAG, "插屏广告关闭");
            }

            @Override
            public void onInterstitialAdVideoStart(ATAdInfo atAdInfo) {
                Log.d(TAG, "插屏广告视频开始播放");
                mInterstitialAd.load();
            }

            @Override
            public void onInterstitialAdVideoEnd(ATAdInfo atAdInfo) {
                Log.d(TAG, "插屏广告视频播放结束");
            }

            @Override
            public void onInterstitialAdVideoError(AdError adError) {
                Log.d(TAG, "插屏广告视频播放失败: " + adError.getCode() + " - " + adError.getDesc());
            }
        });
        
        // 开始加载插屏广告
        mInterstitialAd.load();
    }
    
    /**
     * 展示激励视频广告
     * @param call Capacitor插件调用对象
     */
    @PluginMethod()
    public void ShowRewardVideo(PluginCall call) {
        // 检查激励视频广告是否就绪
        if (mRewardVideoAd != null && mRewardVideoAd.isAdReady()) {
            // 展示激励视频广告
            mRewardVideoAd.show(getActivity());
            
            // 返回展示成功的结果
            JSObject ret = new JSObject();
            ret.put("success", true);
            ret.put("message", "Reward video ad shown");
            call.resolve(ret);
        } else {
            mRewardVideoAd.load();
            // 广告未就绪，返回错误
            call.reject("Reward video ad is not ready");
        }
    }
    
    /**
     * 展示插屏广告
     * @param call Capacitor插件调用对象
     */
    @PluginMethod()
    public void ShowInterstitial(PluginCall call) {
        // 检查插屏广告是否就绪
        if (mInterstitialAd != null && mInterstitialAd.isAdReady()) {
            // 展示插屏广告
            mInterstitialAd.show(getActivity());
            
            // 返回展示成功的结果
            JSObject ret = new JSObject();
            ret.put("success", true);
            ret.put("message", "Interstitial ad shown");
            call.resolve(ret);
        } else {
            mInterstitialAd.load();
            // 广告未就绪，返回错误
            call.reject("Interstitial ad is not ready");
        }
    }
}
