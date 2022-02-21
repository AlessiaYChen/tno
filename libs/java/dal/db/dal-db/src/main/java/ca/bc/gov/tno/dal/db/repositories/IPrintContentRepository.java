package ca.bc.gov.tno.dal.db.repositories;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;

import ca.bc.gov.tno.dal.db.entities.PrintContent;

/**
 * IPrintContentRepository interface, provides a way to interact with the
 * PrintContent repository.
 */
@Repository
public interface IPrintContentRepository extends CrudRepository<PrintContent, Integer> {

}