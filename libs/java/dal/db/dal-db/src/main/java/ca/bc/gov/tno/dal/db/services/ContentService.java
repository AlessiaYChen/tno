package ca.bc.gov.tno.dal.db.services;

import java.util.Arrays;
import java.util.List;
import java.util.Optional;

import org.hibernate.SessionFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import ca.bc.gov.tno.ListHelper;
import ca.bc.gov.tno.auth.PrincipalHelper;
import ca.bc.gov.tno.dal.db.SortDirection;
import ca.bc.gov.tno.dal.db.entities.Content;
import ca.bc.gov.tno.dal.db.entities.ContentAction;
import ca.bc.gov.tno.dal.db.entities.ContentCategory;
import ca.bc.gov.tno.dal.db.entities.ContentTag;
import ca.bc.gov.tno.dal.db.entities.ContentTone;
import ca.bc.gov.tno.dal.db.models.FilterCollection;
import ca.bc.gov.tno.dal.db.models.FilterParam;
import ca.bc.gov.tno.dal.db.models.SortParam;
import ca.bc.gov.tno.dal.db.repositories.IContentActionRepository;
import ca.bc.gov.tno.dal.db.repositories.IContentCategoryRepository;
import ca.bc.gov.tno.dal.db.repositories.IContentRepository;
import ca.bc.gov.tno.dal.db.repositories.IContentTagRepository;
import ca.bc.gov.tno.dal.db.repositories.IContentToneRepository;
import ca.bc.gov.tno.dal.db.repositories.ITimeTrackingRepository;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentActionService;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentCategoryService;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentService;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentTagService;
import ca.bc.gov.tno.dal.db.services.interfaces.IContentToneService;
import ca.bc.gov.tno.dal.db.services.interfaces.IFileReferenceService;
import ca.bc.gov.tno.dal.db.services.interfaces.ITimeTrackingService;
import ca.bc.gov.tno.models.Paged;
import ca.bc.gov.tno.models.interfaces.IPaged;

/**
 * ContentService class, provides a concrete way to interact with content
 * in the database.
 */
@Service
public class ContentService implements IContentService {

  private final SessionFactory sessionFactory;
  private final IContentRepository repository;
  private final IContentActionService contentActionService;
  private final IContentTagService contentTagService;
  private final IContentCategoryService contentCategoryService;
  private final IContentToneService contentToneService;
  private final ITimeTrackingService timeTrackingService;
  private final IFileReferenceService fileReferenceService;

  /**
   * Creates a new instance of a ContentService object, initializes with
   * specified parameters.
   * 
   * @param sessionFactory         The session factory.
   * @param repository             The content repository.
   * @param contentActionService   The content action service.
   * @param contentTagService      The content tag service.
   * @param contentCategoryService The content category service.
   * @param contentToneService     The content tone pool service.
   * @param timeTrackingService    The time tracking service.
   * @param fileReferenceService   The file reference service.
   */
  @Autowired
  public ContentService(final SessionFactory sessionFactory, final IContentRepository repository,
      final IContentActionService contentActionService, final IContentTagService contentTagService,
      final IContentCategoryService contentCategoryService, final IContentToneService contentToneService,
      final ITimeTrackingService timeTrackingService, final IFileReferenceService fileReferenceService) {
    this.sessionFactory = sessionFactory;
    this.repository = repository;
    this.contentActionService = contentActionService;
    this.contentTagService = contentTagService;
    this.contentCategoryService = contentCategoryService;
    this.contentToneService = contentToneService;
    this.timeTrackingService = timeTrackingService;
    this.fileReferenceService = fileReferenceService;
  }

  /**
   * Find all that match the criteria.
   * 
   * @return A list of content.
   */
  @Override
  public List<Content> findAll() {
    var items = (List<Content>) repository.findAll();
    return items;
  }

  /**
   * Find a page of content that match the query.
   * 
   * @param page     The page to pull content from.
   * @param quantity Number of items to return in a page.
   * @param filter   An array of filter parameters.
   * @param sort     An array of sort parameters ['col1 desc', 'col2 asc']
   * @return A page of content.
   */
  public IPaged<Content> find(int page, int quantity, FilterCollection filter, SortParam[] sort) {
    page = page < 1 ? 1 : page;
    quantity = quantity < 1 ? 10 : quantity;

    if (sort == null || sort.length == 0)
      sort = new SortParam[] {
          new SortParam("content", "createdOn", SortDirection.Descending),
          new SortParam("content", "updatedOn", SortDirection.Descending),
          new SortParam("content", "source", SortDirection.Ascending),
          new SortParam("content", "headline", SortDirection.Ascending) };

    var session = sessionFactory.getCurrentSession();
    var ts = session.beginTransaction();

    // TODO: Switch to parameters.
    StringBuilder where = new StringBuilder();
    if (filter != null && filter.getFilters().size() > 0) {
      where.append(" WHERE");
      var filters = filter.getFilters();
      var first = true;
      for (Object op : filters) {
        var param = (FilterParam<?>) op;
        where.append(String.format("%s %s", (!first ? " AND" : ""), param.toString("content")));
        first = false;
      }
    }

    try {
      var order = " ORDER BY "
          + String.join(", ", Arrays.stream(sort).map(s -> s.toString("content")).toArray(String[]::new));
      var pageSql = """
          SELECT DISTINCT content FROM Content content
          JOIN FETCH content.contentType AS contentType
          JOIN FETCH content.mediaType AS mediaType
          JOIN FETCH content.license AS license
          LEFT JOIN FETCH content.owner AS owner
          LEFT JOIN FETCH content.dataSource AS dataSource
          """ + where.toString() + order;
      var pageQuery = session.createQuery(pageSql)
          .setFirstResult((page - 1) * quantity)
          .setMaxResults(quantity);
      var items = pageQuery.getResultList();

      var totalSql = "SELECT COUNT(*) FROM Content content" + where.toString();
      var totalQuery = session.createQuery(totalSql);
      var total = (long) totalQuery.uniqueResult();

      return new Paged<Content>(ListHelper.castList(Content.class, items), page, quantity, total);
    } finally {
      ts.commit();
    }
  }

  /**
   * Find the content for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content if it exists.
   */
  @Override
  public Optional<Content> findById(int key) {
    var reference = repository.findById(key);
    return reference;
  }

  /**
   * Find the content for the specified primary key.
   * 
   * @param key The primary key.
   * @return A new instance of the content if it exists.
   */
  @Override
  public Optional<Content> findById(int key, Boolean eager) {
    if (eager) {
      var session = sessionFactory.getCurrentSession();
      var ts = session.beginTransaction();
      try {

        var sql = """
            SELECT DISTINCT content FROM Content content
            JOIN FETCH content.contentType AS contentType
            JOIN FETCH content.mediaType AS mediaType
            JOIN FETCH content.license AS license
            LEFT JOIN FETCH content.owner AS owner
            LEFT JOIN FETCH content.dataSource AS dataSource
            LEFT JOIN FETCH content.timeTrackings
            WHERE content.id=:id
            """;
        var find = session.createQuery(sql)
            .setParameter("id", key);
        var result = Optional.ofNullable((Content) find.uniqueResult());

        if (result.isPresent()) {
          var tags = session
              .createQuery("SELECT ct FROM ContentTag ct JOIN FETCH ct.tag AS t WHERE ct.contentId=:id")
              .setParameter("id", key)
              .getResultList();
          result.get().setContentTags(ListHelper.castList(ContentTag.class, tags));

          var actions = session
              .createQuery("SELECT ca FROM ContentAction ca JOIN FETCH ca.action AS a WHERE ca.contentId=:id")
              .setParameter("id", key)
              .getResultList();
          result.get().setContentActions(ListHelper.castList(ContentAction.class, actions));

          var categories = session
              .createQuery("SELECT cc FROM ContentCategory cc JOIN FETCH cc.category AS c WHERE cc.contentId=:id")
              .setParameter("id", key)
              .getResultList();
          result.get().setContentCategories(ListHelper.castList(ContentCategory.class, categories));

          var tonePools = session
              .createQuery("SELECT ct FROM ContentTone ct JOIN FETCH ct.tonePool AS tp WHERE ct.contentId=:id")
              .setParameter("id", key)
              .getResultList();
          result.get().setContentTonePools(ListHelper.castList(ContentTone.class, tonePools));
        }

        return result;
      } finally {
        ts.commit();
      }
    }

    return findById(key);
  }

  /**
   * Add a new content to the data source.
   * 
   * @param entity The content to add.
   * @return A new instance of the content that was added.
   */
  @Override
  public Content add(Content entity) {
    var result = repository.save(PrincipalHelper.addAudit(entity));

    if (entity.getContentActions().size() > 0) {
      contentActionService.add(entity.getContentActions().stream().map((ca) -> {
        ca.setContentId(result.getId());
        return ca;
      }).toList());
    }
    if (entity.getContentTags().size() > 0) {
      contentTagService.add(entity.getContentTags().stream().map((ca) -> {
        ca.setContentId(result.getId());
        return ca;
      }).toList());
    }
    if (entity.getContentCategories().size() > 0) {
      contentCategoryService.add(entity.getContentCategories().stream().map((ca) -> {
        ca.setContentId(result.getId());
        return ca;
      }).toList());
    }
    if (entity.getContentTonePools().size() > 0) {
      contentToneService.add(entity.getContentTonePools().stream().map((ca) -> {
        ca.setContentId(result.getId());
        return ca;
      }).toList());
    }
    return result;
  }

  /**
   * Update the specified content in the data source.
   * 
   * @param entity The content to update.
   * @return A new instance of the content that was updated.
   */
  @Override
  public Content update(Content entity) {
    var result = repository.save(PrincipalHelper.updateAudit(entity));

    if (entity.getContentActions().size() > 0) {
      contentActionService.update(entity.getContentActions());
    }
    if (entity.getContentTags().size() > 0) {
      contentTagService.update(entity.getContentTags());
    }
    if (entity.getContentCategories().size() > 0) {
      contentCategoryService.update(entity.getContentCategories());
    }
    if (entity.getContentTonePools().size() > 0) {
      contentToneService.update(entity.getContentTonePools());
    }
    return result;
  }

  /**
   * Delete the specified content from the data source.
   * 
   * @param entity The content to delete.
   */
  @Override
  public void delete(Content entity) {
    var actions = contentActionService.findById(entity.getId());
    contentActionService.delete(actions);
    var tags = contentTagService.findById(entity.getId());
    contentTagService.delete(tags);
    var categories = contentCategoryService.findById(entity.getId());
    contentCategoryService.delete(categories);
    var tonePools = contentToneService.findById(entity.getId());
    contentToneService.delete(tonePools);
    var time = timeTrackingService.findByContentId(entity.getId());
    timeTrackingService.delete(time);
    var files = fileReferenceService.findByContentId(entity.getId());
    fileReferenceService.delete(files);
    repository.delete(entity);
  }

}
