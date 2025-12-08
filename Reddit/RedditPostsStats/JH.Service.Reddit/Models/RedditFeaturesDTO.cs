public class RedditFeaturesDTO
{
    public Features features { get; set; }
}

public class Features
{
    public bool modmail_harassment_filter { get; set; }
    public bool mod_service_mute_writes { get; set; }
    public bool promoted_trend_blanks { get; set; }
    public bool show_amp_link { get; set; }
    public MwebCommentsBanner mweb_comments_banner { get; set; }
    public bool is_email_permission_required { get; set; }
    public bool mod_awards { get; set; }
    public bool expensive_coins_package { get; set; }
    public bool chat_subreddit { get; set; }
    public bool awards_on_streams { get; set; }
    public bool mweb_xpromo_modal_listing_click_daily_dismissible_ios { get; set; }
    public bool cookie_consent_banner { get; set; }
    public bool modlog_copyright_removal { get; set; }
    public bool do_not_track { get; set; }
    public bool images_in_comments { get; set; }
    public bool mod_service_mute_reads { get; set; }
    public bool chat_user_settings { get; set; }
    public bool use_pref_account_deployment { get; set; }
    public bool mweb_xpromo_interstitial_comments_ios { get; set; }
    public MwebSharingClipboard mweb_sharing_clipboard { get; set; }
    public bool premium_subscriptions_table { get; set; }
    public bool mweb_xpromo_interstitial_comments_android { get; set; }
    public bool crowd_control_for_post { get; set; }
    public bool mweb_xpromo_modal_listing_click_daily_dismissible_android { get; set; }
    public MwebFooterUpsell mweb_footer_upsell { get; set; }
    public bool chat_group_rollout { get; set; }
    public bool resized_styles_images { get; set; }
    public bool noreferrer_to_noopener { get; set; }
    public SwapStepsTwoAndThreeRecalibration swap_steps_two_and_three_recalibration { get; set; }
}

public class MwebCommentsBanner
{
    public string owner { get; set; }
    public string variant { get; set; }
    public int experiment_id { get; set; }
}

public class MwebSharingClipboard
{
    public string owner { get; set; }
    public string variant { get; set; }
    public int experiment_id { get; set; }
}

public class MwebFooterUpsell
{
    public string owner { get; set; }
    public string variant { get; set; }
    public int experiment_id { get; set; }
}

public class SwapStepsTwoAndThreeRecalibration
{
    public string owner { get; set; }
    public string variant { get; set; }
    public int experiment_id { get; set; }
}
