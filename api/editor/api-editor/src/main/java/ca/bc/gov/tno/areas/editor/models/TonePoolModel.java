package ca.bc.gov.tno.areas.editor.models;

import ca.bc.gov.tno.dal.db.entities.TonePool;
import ca.bc.gov.tno.models.AuditColumnModel;

public class TonePoolModel extends AuditColumnModel {
  /**
   * Primary key to identify the tone pool.
   */
  private int id;

  /**
   * A unique name to identify the tone pool.
   */
  private String name;

  /**
   * A description of the tone pool.
   */
  private String description = "";

  /**
   * Foreign key to User who owns this tone pool.
   */
  private int ownerId;

  /**
   * The owner reference.
   */
  private UserModel owner;

  /**
   * The order to display.
   */
  private int sortOrder;

  /**
   * Whether this record is visible to other users.
   */
  private boolean shared = true;

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean enabled = true;

  /**
   * A collection of role tone pools that belong to this tone pool.
   */
  // private List<ContentTone> contentTones = new ArrayList<>();

  public TonePoolModel() {
  }

  public TonePoolModel(TonePool entity) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.name = entity.getName();
      this.description = entity.getDescription();
      this.ownerId = entity.getOwnerId();
      this.owner = new UserModel(entity.getOwner());
      this.shared = entity.isShared();
      this.enabled = entity.isEnabled();
      this.sortOrder = entity.getSortOrder();
    }
  }

  /**
   * @return int the id
   */
  public int getId() {
    return id;
  }

  /**
   * @param id the id to set
   */
  public void setId(int id) {
    this.id = id;
  }

  /**
   * @return String the name
   */
  public String getName() {
    return name;
  }

  /**
   * @param name the name to set
   */
  public void setName(String name) {
    this.name = name;
  }

  /**
   * @return String the description
   */
  public String getDescription() {
    return description;
  }

  /**
   * @param description the description to set
   */
  public void setDescription(String description) {
    this.description = description;
  }

  /**
   * @return boolean the enabled
   */
  public boolean isEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  /**
   * @return int the sortOrder
   */
  public int getSortOrder() {
    return sortOrder;
  }

  /**
   * @param sortOrder the sortOrder to set
   */
  public void setSortOrder(int sortOrder) {
    this.sortOrder = sortOrder;
  }

}
